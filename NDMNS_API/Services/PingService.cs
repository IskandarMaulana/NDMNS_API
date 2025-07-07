using Microsoft.AspNetCore.SignalR;
using NDMNS_API.AppHubs;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace NDMNS_API.Services
{
    public class PingService : BackgroundService
    {
        private readonly IHubContext<MonitoringHub> _hubContext;
        private readonly NetworkRepository _networkRepository;
        private readonly SettingRepository _settingRepository;
        private readonly ILogger<PingService> _logger;

        public PingService(
            IConfiguration configuration,
            IHubContext<MonitoringHub> hubContext,
            ILogger<PingService> logger
        )
        {
            _hubContext = hubContext;
            _networkRepository = new NetworkRepository(configuration);
            _settingRepository = new SettingRepository(configuration);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var sitenetworks = await GetNetworksWithPingData();
                    await _hubContext.Clients.All.SendAsync(
                        "Ping",
                        sitenetworks,
                        cancellationToken: stoppingToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during network monitoring cycle");
                }

                var pingInterval = GetSettingValue("PING_INTERVAL", 300000);
                await Task.Delay(pingInterval, stoppingToken);
            }
        }

        public async Task<List<NetworkViewModel>> GetNetworksWithPingData()
        {
            List<NetworkViewModel> networks =
                _networkRepository.GetAllNetworks().data ?? new List<NetworkViewModel>();

            var totalTimeout = GetSettingValue("TOTAL_MONITORING_TIMEOUT", 300000);
            using var cts = new CancellationTokenSource(totalTimeout);

            var tasks = new List<Task>();

            foreach (var network in networks)
            {
                tasks.Add(UpdateNetworkWithPingData(network, cts.Token));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Network monitoring operation timed out after {Timeout}ms",
                    totalTimeout
                );
            }

            foreach (var network in networks)
            {
                var net = new Network
                {
                    Id = network.Id,
                    Status = network.Status,
                    Latency = network.Latency,
                    LastUpdate = network.LastUpdate,
                };
                _networkRepository.UpdateNetworkPing(net);
            }
            return networks;
        }

        private async Task UpdateNetworkWithPingData(
            NetworkViewModel network,
            CancellationToken cancellationToken = default
        )
        {
            decimal averageLatency = 0;

            try
            {
                var pingResult = await PingIcmpAsync(network.Ip, cancellationToken);

                // Update network status based on ping result
                network.Latency = pingResult.AverageLatency;

                if (network.Status != pingResult.Status)
                {
                    network.Status = pingResult.Status;
                    var now = DateTime.Now;
                    if (now >= new DateTime(1753, 1, 1) && now <= new DateTime(9999, 12, 31))
                    {
                        network.LastUpdate = now;
                    }
                }

                // Log ping result for debugging
                _logger.LogDebug(
                    "Ping result for {IP}: Status={Status}, Latency={Latency}ms, Success={Success}%, Response={Response}",
                    network.Ip,
                    pingResult.Status,
                    pingResult.AverageLatency,
                    pingResult.SuccessPercentage,
                    pingResult.Response
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Ping operation timed out for {IP}", network.Ip);

                network.Latency = averageLatency;

                if (network.Status != 1)
                {
                    network.Status = 1;
                    network.LastUpdate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring network {IP}", network.Ip);

                network.Latency = averageLatency;

                if (network.Status != 1)
                {
                    network.Status = 1;
                    var now = DateTime.Now;
                    if (now >= new DateTime(1753, 1, 1) && now <= new DateTime(9999, 12, 31))
                    {
                        network.LastUpdate = now;
                    }
                }
            }
        }

        private async Task<PingResult> PingIcmpAsync(
            string hostname,
            CancellationToken cancellationToken = default
        )
        {
            var result = new PingResult
            {
                Hostname = hostname,
                Status = 1, // Default to Down
                Response = "ICMP Ping timed out",
            };

            if (string.IsNullOrEmpty(hostname))
            {
                result.Response = "Destination address not specified";
                return result;
            }

            try
            {
                // Get configuration values
                int pingAttempts = GetSettingValue("PING_ATTEMPTS", 20);
                int timeout = GetSettingValue("PING_TIMEOUT", 3000);
                int pingDelay = GetSettingValue("PING_DELAY", 0);
                int maxPingTime = GetSettingValue("MAX_PING_TIME", 60000);

                // Clean up hostname (remove transport prefixes if any)
                hostname = StripIpAddress(hostname);

                // Resolve hostname to IP if needed
                IPAddress targetIP;
                if (!IPAddress.TryParse(hostname, out targetIP))
                {
                    try
                    {
                        IPHostEntry hostEntry = await Dns.GetHostEntryAsync(
                            hostname,
                            cancellationToken
                        );
                        if (hostEntry.AddressList.Length == 0)
                        {
                            result.Response =
                                $"ICMP Ping Error: DNS resolution failed for {hostname}";
                            return result;
                        }
                        targetIP = hostEntry.AddressList[0];
                    }
                    catch (Exception ex)
                    {
                        result.Response =
                            $"ICMP Ping Error: DNS resolution failed for {hostname} - {ex.Message}";
                        return result;
                    }
                }

                // Create timeout for the entire ping operation
                using var pingCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken
                );
                pingCts.CancelAfter(maxPingTime);

                // Perform ping with retries
                using var ping = new Ping();
                int successCount = 0;
                long totalLatency = 0;

                // Create test data (equivalent to PHP's bin2hex('cacti-monitoring-system'))
                byte[] buffer = Encoding.ASCII.GetBytes("network-down-monitoring-system");

                PingOptions options = new PingOptions();
                options.DontFragment = true;

                for (int i = 0; i < pingAttempts && !pingCts.Token.IsCancellationRequested; i++)
                {
                    try
                    {
                        var reply = await ping.SendPingAsync(targetIP, timeout, buffer, options);

                        if (reply.Status == IPStatus.Success)
                        {
                            successCount++;
                            totalLatency += reply.RoundtripTime;
                        }
                        else
                        {
                            _logger.LogDebug(
                                "Ping attempt {Attempt} failed for {IP}: {Status}",
                                i + 1,
                                hostname,
                                reply.Status
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(
                            ex,
                            "Ping attempt {Attempt} failed for {IP}",
                            i + 1,
                            hostname
                        );
                    }

                    // Add delay between pings (except for last attempt)
                    if (pingDelay > 0 && i < pingAttempts - 1)
                    {
                        try
                        {
                            await Task.Delay(pingDelay, pingCts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }

                // Calculate results
                if (successCount > 0)
                {
                    result.AverageLatency = (decimal)totalLatency / successCount;
                }

                result.SuccessPercentage =
                    pingAttempts > 0 ? (double)successCount / pingAttempts * 100 : 0;

                // Determine status based on success percentage
                if (result.SuccessPercentage >= 80) // 80% success
                {
                    result.Status = 2; // Up
                    result.Response =
                        $"ICMP Ping Success ({result.AverageLatency:F1} ms avg, {result.SuccessPercentage:F1}% success)";
                }
                else if (result.SuccessPercentage <= 20) // 20% success
                {
                    result.Status = 1; // Down
                    result.Response = $"ICMP Ping Failed ({result.SuccessPercentage:F1}% success)";
                }
                else
                {
                    result.Status = 3; // Intermittent
                    result.Response =
                        $"ICMP Ping Intermittent ({result.AverageLatency:F1} ms avg, {result.SuccessPercentage:F1}% success)";
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Ping operation timed out for {IP}", hostname);
                result.Response = "ICMP ping operation timed out";
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing ICMP ping for {IP}", hostname);
                result.Response = $"ICMP Ping Error: {ex.Message}";
                return result;
            }
        }

        private string StripIpAddress(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
                return hostname;

            // Remove common transport prefixes
            string[] prefixes = { "tcp://", "udp://", "http://", "https://", "snmp://" };

            foreach (var prefix in prefixes)
            {
                if (hostname.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    hostname = hostname.Substring(prefix.Length);
                    break;
                }
            }

            // Remove port if present (but be careful with IPv6 addresses)
            if (!hostname.StartsWith("[")) // Not IPv6 with brackets
            {
                int colonIndex = hostname.LastIndexOf(':');
                if (colonIndex > 0)
                {
                    string portPart = hostname.Substring(colonIndex + 1);
                    if (int.TryParse(portPart, out _))
                    {
                        hostname = hostname.Substring(0, colonIndex);
                    }
                }
            }
            else
            {
                // Handle IPv6 with port: [2001:db8::1]:8080
                int bracketEnd = hostname.IndexOf(']');
                if (bracketEnd > 0 && bracketEnd < hostname.Length - 1)
                {
                    string afterBracket = hostname.Substring(bracketEnd + 1);
                    if (
                        afterBracket.StartsWith(":")
                        && int.TryParse(afterBracket.Substring(1), out _)
                    )
                    {
                        hostname = hostname.Substring(0, bracketEnd + 1);
                    }
                }
            }

            return hostname;
        }

        private int GetSettingValue(string code, int defaultValue)
        {
            try
            {
                List<SettingViewModel> settings = _settingRepository.GetAllSettings();
                var setting = settings.FirstOrDefault(s => s.Code == code);

                if (setting != null && int.TryParse(setting.Value, out int value))
                {
                    return value;
                }

                _logger.LogWarning(
                    "Setting with code {Code} not found or invalid, using default value {DefaultValue}",
                    code,
                    defaultValue
                );
                return defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving setting {Code}, using default value {DefaultValue}",
                    code,
                    defaultValue
                );
                return defaultValue;
            }
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using NDMNS_API.AppHubs;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace NDMNS_API.Services
{
    public class SystemHealthService : BackgroundService
    {
        private readonly IHubContext<MonitoringHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SystemHealthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly SettingRepository _settingRepository;

        public SystemHealthService(
            IConfiguration configuration,
            IHubContext<MonitoringHub> hubContext,
            ILogger<SystemHealthService> logger,
            HttpClient httpClient
        )
        {
            _configuration = configuration;
            _hubContext = hubContext;
            _logger = logger;
            _httpClient = httpClient;
            _settingRepository = new SettingRepository(configuration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var healthStatus = await GetSystemHealthStatus();
                    await _hubContext.Clients.All.SendAsync(
                        "SystemHealth",
                        healthStatus,
                        cancellationToken: stoppingToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during system health monitoring cycle");
                }

                // Check every 5 minutes (300000ms)
                var healthCheckInterval = GetSettingValue("HEALTH_CHECK_INTERVAL", 60000);
                await Task.Delay(healthCheckInterval, stoppingToken);
            }
        }

        public async Task<SystemHealthResult> GetSystemHealthStatus()
        {
            var healthStatus = new SystemHealthResult
            {
                CheckTime = DateTime.Now,
                Database = new HealthCheckResult
                {
                    Name = "SQL Server Database",
                    Status = "Unknown",
                },
                Internet = new HealthCheckResult
                {
                    Name = "Internet Connection",
                    Status = "Unknown",
                },
                NodeJsService = new HealthCheckResult
                {
                    Name = "Node.js WhatsApp Service",
                    Status = "Unknown",
                },
                WhatsAppService = new HealthCheckResult
                {
                    Name = "WhatsApp Service",
                    Status = "Unknown",
                },
            };

            var timeout = GetSettingValue("HEALTH_CHECK_TIMEOUT", 30000);
            using var cts = new CancellationTokenSource(timeout);

            var tasks = new List<Task>
            {
                CheckDatabaseStatusAsync(healthStatus.Database, cts.Token),
                CheckInternetStatusAsync(healthStatus.Internet, cts.Token),
                CheckNodeJsServiceStatusAsync(healthStatus.NodeJsService, cts.Token),
                CheckWhatsAppServiceStatusAsync(healthStatus.WhatsAppService, cts.Token),
            };

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Health check operation timed out after {Timeout}ms", timeout);
            }

            // Log overall health status
            _logger.LogInformation(
                "System Health Check - Database: {DatabaseStatus}, Internet: {InternetStatus}, NodeJS: {NodeJsStatus}, WhatsApp: {WhatsAppServiceStatus}",
                healthStatus.Database.Status,
                healthStatus.Internet.Status,
                healthStatus.NodeJsService.Status,
                healthStatus.WhatsAppService.Status
            );

            return healthStatus;
        }

        private async Task CheckDatabaseStatusAsync(
            HealthCheckResult result,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    result.Status = "Error";
                    result.Message = "Database connection string not configured";
                    result.ResponseTime = 0;
                    return;
                }

                var startTime = DateTime.Now;
                using var connection = new SqlConnection(connectionString);

                // Set command timeout
                using var command = new SqlCommand("SELECT 1", connection);
                command.CommandTimeout = 30;

                await connection.OpenAsync(cancellationToken);
                await command.ExecuteScalarAsync(cancellationToken);

                var responseTime = (DateTime.Now - startTime).TotalMilliseconds;

                result.Status = "Healthy";
                result.Message = $"Database connection successful";
                result.ResponseTime = (decimal)responseTime;

                _logger.LogDebug(
                    "Database health check successful in {ResponseTime}ms",
                    responseTime
                );
            }
            catch (OperationCanceledException)
            {
                result.Status = "Timeout";
                result.Message = "Database connection timed out";
                result.ResponseTime = 0;
                _logger.LogWarning("Database health check timed out");
            }
            catch (Exception ex)
            {
                result.Status = "Error";
                result.Message = $"Database connection failed: {ex.Message}";
                result.ResponseTime = 0;
                _logger.LogError(ex, "Database health check failed");
            }
        }

        private async Task CheckInternetStatusAsync(
            HealthCheckResult result,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var startTime = DateTime.Now;

                // Test multiple reliable DNS servers
                var testHosts = new[] { "8.8.8.8", "1.1.1.1", "208.67.222.222" };
                var successCount = 0;
                var totalLatency = 0L;

                using var ping = new Ping();
                var timeout = GetSettingValue("PING_TIMEOUT", 3000);

                foreach (var host in testHosts)
                {
                    try
                    {
                        var reply = await ping.SendPingAsync(host, timeout);
                        if (reply.Status == IPStatus.Success)
                        {
                            successCount++;
                            totalLatency += reply.RoundtripTime;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Ping to {Host} failed", host);
                    }
                }

                var responseTime = (DateTime.Now - startTime).TotalMilliseconds;

                if (successCount > 0)
                {
                    var averageLatency = totalLatency / successCount;
                    result.Status = "Healthy";
                    result.Message =
                        $"Internet connection available ({successCount}/{testHosts.Length} hosts reachable, avg {averageLatency}ms)";
                    result.ResponseTime = (decimal)responseTime;
                }
                else
                {
                    result.Status = "Error";
                    result.Message = "No internet connectivity detected";
                    result.ResponseTime = (decimal)responseTime;
                }

                _logger.LogDebug(
                    "Internet health check completed - {SuccessCount}/{TotalHosts} hosts reachable",
                    successCount,
                    testHosts.Length
                );
            }
            catch (OperationCanceledException)
            {
                result.Status = "Timeout";
                result.Message = "Internet connectivity check timed out";
                result.ResponseTime = 0;
                _logger.LogWarning("Internet health check timed out");
            }
            catch (Exception ex)
            {
                result.Status = "Error";
                result.Message = $"Internet connectivity check failed: {ex.Message}";
                result.ResponseTime = 0;
                _logger.LogError(ex, "Internet health check failed");
            }
        }

        private async Task CheckNodeJsServiceStatusAsync(
            HealthCheckResult result,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var baseUrl = _configuration["WhatsAppService:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    result.Status = "Error";
                    result.Message = "Node.js service URL not configured";
                    result.ResponseTime = 0;
                    return;
                }

                var startTime = DateTime.Now;

                var healthCheckUrl = $"{baseUrl.TrimEnd('/')}/api/health";

                using var response = await _httpClient.GetAsync(healthCheckUrl, cancellationToken);
                var responseTime = (DateTime.Now - startTime).TotalMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    result.Status = "Healthy";
                    result.Message =
                        $"Node.js service is running (HTTP {(int)response.StatusCode})";
                    result.ResponseTime = (decimal)responseTime;

                    try
                    {
                        var healthData = JsonSerializer.Deserialize<JsonElement>(content);
                        if (healthData.TryGetProperty("status", out var statusProp))
                        {
                            result.Message += $" - Status: {statusProp.GetString()}";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    result.Status = "Error";
                    result.Message = $"Node.js service returned HTTP {(int)response.StatusCode}";
                    result.ResponseTime = (decimal)responseTime;
                }

                _logger.LogDebug(
                    "Node.js service health check completed in {ResponseTime}ms with status {StatusCode}",
                    responseTime,
                    response.StatusCode
                );
            }
            catch (HttpRequestException ex)
            {
                result.Status = "Error";
                result.Message = $"Node.js service unreachable: {ex.Message}";
                result.ResponseTime = 0;
                _logger.LogWarning(ex, "Node.js service health check failed - service unreachable");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                result.Status = "Timeout";
                result.Message = "Node.js service health check timed out";
                result.ResponseTime = 0;
                _logger.LogWarning("Node.js service health check timed out");
            }
            catch (OperationCanceledException)
            {
                result.Status = "Timeout";
                result.Message = "Node.js service health check was cancelled";
                result.ResponseTime = 0;
                _logger.LogWarning("Node.js service health check was cancelled");
            }
            catch (Exception ex)
            {
                result.Status = "Error";
                result.Message = $"Node.js service health check failed: {ex.Message}";
                result.ResponseTime = 0;
                _logger.LogError(ex, "Node.js service health check failed");
            }
        }

        private async Task CheckWhatsAppServiceStatusAsync(
            HealthCheckResult result,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var baseUrl = _configuration["WhatsAppService:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    result.Status = "Error";
                    result.Message = "WhatsApp Service URL not configured";
                    result.ResponseTime = 0;
                    return;
                }

                var startTime = DateTime.Now;

                var healthCheckUrl = $"{baseUrl.TrimEnd('/')}/api/whatsapp/qr";

                using var response = await _httpClient.GetAsync(healthCheckUrl, cancellationToken);
                var responseTime = (DateTime.Now - startTime).TotalMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        result.Message =
                            $"WhatsApp Service is running (HTTP {(int)response.StatusCode})";
                        result.ResponseTime = (decimal)responseTime;

                        var healthData = JsonSerializer.Deserialize<JsonElement>(content);
                        if (healthData.TryGetProperty("status", out var statusProp))
                        {
                            if (statusProp.GetString().Equals("connected"))
                            {
                                result.Status = "Healthy";
                            }
                            else if (statusProp.GetString().Equals("authenticated"))
                            {
                                result.Status = "Authenticated";
                            }
                            else if (statusProp.GetString().Equals("initializing"))
                            {
                                result.Status = "Initializing";
                            }
                            else if (statusProp.GetString().Equals("disconnected"))
                            {
                                result.Status = "Disconnected";
                            }
                            else if (statusProp.GetString().Equals("auth_failed"))
                            {
                                result.Status = "Auth Failed";
                            }
                            else if (statusProp.GetString().Equals("qr_ready"))
                            {
                                result.Status = "Qr Ready";
                            }
                            else if (statusProp.GetString().Equals("init_failed"))
                            {
                                result.Status = "Init Failed";
                            }
                            else if (statusProp.GetString().Equals("setup_failed"))
                            {
                                result.Status = "Setup Failed";
                            }
                            else if (statusProp.GetString().Equals("max_attempts_reached"))
                            {
                                result.Status = "Max Attempts Reached";
                            }
                            else
                            {
                                result.Status = "Error";
                            }
                            result.Message += $" - Status: {statusProp.GetString()}";
                        }
                        else
                        {
                            result.Status = "Disconnected";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    result.Status = "Error";
                    result.Message = $"WhatsApp Service returned HTTP {(int)response.StatusCode}";
                    result.ResponseTime = (decimal)responseTime;
                }

                _logger.LogDebug(
                    "WhatsApp Service health check completed in {ResponseTime}ms with status {StatusCode}",
                    responseTime,
                    response.StatusCode
                );
            }
            catch (HttpRequestException ex)
            {
                result.Status = "Error";
                result.Message = $"WhatsApp Service unreachable: {ex.Message}";
                result.ResponseTime = 0;
                _logger.LogWarning(
                    ex,
                    "WhatsApp Service health check failed - service unreachable"
                );
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                result.Status = "Timeout";
                result.Message = "WhatsApp Service health check timed out";
                result.ResponseTime = 0;
                _logger.LogWarning("WhatsApp Service health check timed out");
            }
            catch (OperationCanceledException)
            {
                result.Status = "Timeout";
                result.Message = "WhatsApp Service health check was cancelled";
                result.ResponseTime = 0;
                _logger.LogWarning("WhatsApp Service health check was cancelled");
            }
            catch (Exception ex)
            {
                result.Status = "Error";
                result.Message = $"WhatsApp Service health check failed: {ex.Message}";
                result.ResponseTime = 0;
                _logger.LogError(ex, "WhatsApp Service health check failed");
            }
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

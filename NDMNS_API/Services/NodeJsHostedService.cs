using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NDMNS_API.Services
{
    public class NodeJsHostedService : BackgroundService
    {
        private readonly ILogger<NodeJsHostedService> _logger;
        private readonly IConfiguration _configuration;
        private Process _nodeProcess;
        private readonly string _nodeProjectPath;
        private readonly string _npmCommand;

        public NodeJsHostedService(
            ILogger<NodeJsHostedService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _nodeProjectPath = _configuration["NodeJs:ProjectPath"] ?? "wwwroot/nodejs-app";
            _npmCommand = _configuration["NodeJs:Command"] ?? "start";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await StartNodeProcess(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Node.js process crashed, restarting in 5 seconds...");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        public async Task StartNodeProcess(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Starting Node.js application on path: {_nodeProjectPath}");

                if (!Directory.Exists(_nodeProjectPath))
                {
                    _logger.LogError($"Node.js project path not found: {_nodeProjectPath}");
                    return;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = GetNpmExecutable(),
                    Arguments = $"run {_npmCommand}",
                    WorkingDirectory = Path.GetFullPath(_nodeProjectPath),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _nodeProcess = new Process { StartInfo = processStartInfo };

                _nodeProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.LogInformation($"Node.js: {e.Data}");
                };

                _nodeProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.LogError($"Node.js Error: {e.Data}");
                };

                _nodeProcess.Start();
                _nodeProcess.BeginOutputReadLine();
                _nodeProcess.BeginErrorReadLine();

                _logger.LogInformation($"Node.js application started with PID: {_nodeProcess.Id}");

                await Task.Delay(2000, cancellationToken);

                if (_nodeProcess.HasExited)
                {
                    _logger.LogError($"Node.js process exited immediately with code: {_nodeProcess.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Node.js application");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_nodeProcess != null && !_nodeProcess.HasExited)
                {
                    _logger.LogInformation("Stopping Node.js application...");

                    // Coba stop gracefully dulu
                    _nodeProcess.CloseMainWindow();

                    // Tunggu 5 detik untuk graceful shutdown
                    if (!_nodeProcess.WaitForExit(5000))
                    {
                        _logger.LogWarning("Node.js process didn't exit gracefully, forcing kill...");
                        _nodeProcess.Kill();
                    }

                    _logger.LogInformation("Node.js application stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Node.js application");
            }

            await Task.CompletedTask;
        }

        private string GetNpmExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "npm.cmd";
            }

            return "npm";
        }

        public void Dispose()
        {
            _nodeProcess?.Dispose();
        }
    }
}

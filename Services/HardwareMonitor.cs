using System.IO;
using HardwareMonitor.IServices;
using HardwareMonitor.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace HardwareMonitor.Services
{
    public class HardwareMonitor : IHardwareMonitor
    {
        private readonly IOptionsMonitor<HardwareMonitorOptions> _options;
        private readonly ILogger<HardwareMonitor> _logger;
        public event Action? FileChanged;
        private FileSystemWatcher? _watcher;
        private DateTimeOffset _lastFilehanged = DateTimeOffset.MinValue;

        public HardwareMonitor(IOptionsMonitor<HardwareMonitorOptions> options, ILogger<HardwareMonitor> logger)
        {
            _options = options;
            _logger = logger;
            _logger.LogDebug("HardwareMonitor constructed.");
        }

        public void StartMonitoring()
        {
            if (_watcher != null)
            {
                _logger.LogDebug("StartMonitoring called but watcher already started.");
                return; // already started
            }

            var filePath = _options.CurrentValue.HardwareFilePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogError("HardwareFilePath is not configured.");
                throw new InvalidOperationException("HardwareFilePath is not configured.");
            }

            var directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                _logger.LogError("Configured HardwareFilePath is invalid: {FilePath}", filePath);
                throw new InvalidOperationException("Configured HardwareFilePath is invalid.");
            }

            try
            {
                _watcher = new FileSystemWatcher(directory)
                {
                    Filter = Path.GetFileName(filePath),
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true,
                };

                _watcher.Changed += OnChanged;
                _logger.LogInformation("FileSystemWatcher started for {Path}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start FileSystemWatcher for {Path}", filePath);
                throw;
            }
        }

        public void StopMonitoring()
        {
            if (_watcher == null)
            {
                _logger.LogDebug("StopMonitoring called but watcher is not active.");
                return;
            }

            try
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnChanged;
                _watcher.Dispose();
                _watcher = null;
                _logger.LogInformation("FileSystemWatcher stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while stopping FileSystemWatcher.");
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if(_lastFilehanged + TimeSpan.FromMilliseconds(200) > DateTimeOffset.Now)
            {
                // Debounce rapid successive events
                return;
            }
            _logger.LogDebug($"File change detected: {e.FullPath}");
            FileChanged?.Invoke();
            _lastFilehanged = DateTimeOffset.Now;
        }

        public void Dispose()
        {
            StopMonitoring();
            GC.SuppressFinalize(this);
            _logger.LogDebug("HardwareMonitor disposed.");
        }
    }
}

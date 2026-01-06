using HardwareMonitor.IServices;
using HardwareMonitor.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reactive.Subjects;

namespace HardwareMonitor.Services;

public class DataProvider : IDataProvider
{
    private readonly IHardwareMonitor _hardwareMonitor;
    private readonly IOptionsMonitor<HardwareMonitorOptions> _options;
    private readonly ILogger<DataProvider> _logger;
    private readonly BehaviorSubject<List<DataItem>> _dataForDisplaySubject = new BehaviorSubject<List<DataItem>>(new List<DataItem>());

    public IObservable<List<DataItem>> DataItemsStream => _dataForDisplaySubject;

    public DataProvider(IHardwareMonitor monitor, IOptionsMonitor<HardwareMonitorOptions> options, ILogger<DataProvider> logger)
    {
        _options = options;
        _hardwareMonitor = monitor;
        _logger = logger;
        OnFileChanged(); // For the first reading
        _hardwareMonitor.FileChanged += OnFileChanged;
        _logger.LogDebug("DataProvider constructed.");
    }

    private void NotifyFileChanged()
    {
        var filePath = _options.CurrentValue.HardwareFilePath;
        int retries = 3;
        _logger.LogDebug("NotifyFileChanged invoked for {Path}", filePath);

        while (retries-- > 0)
        {
            try
            {   
                var data = ReadFromFile(filePath);
                _dataForDisplaySubject.OnNext(data);
                _logger.LogInformation($"Published {data.Count} data items from {filePath}");
                break;
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, $"IO error reading {filePath}, retries left: {retries}");
                Thread.Sleep(50); // small delay
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error reading {filePath}");
                throw;
            }
        }
    }

    private void OnFileChanged()
    {
        NotifyFileChanged();
    }

    public List<DataItem> ReadFromFile(string path)
    {
        _logger.LogDebug($"ReadFromFile started for {path}");
        var items = new List<DataItem>();

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _logger.LogWarning($"ReadFromFile: path is empty or file does not exist: {path}");
            return items;
        }

        // Open the file with explicit FileShare so other processes can read/write
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string name = string.Empty;
                string valueStr = string.Empty;
                object value = string.Empty;

                if (parts.Length == 2)
                {
                    name = parts[0];
                    valueStr = parts[1];
                }
                else if (parts.Length == 1)
                {
                    valueStr = parts[0];
                }

                // Detect type
                if (bool.TryParse(valueStr, out var b)) value = b;
                else if (int.TryParse(valueStr, out var i)) value = i;
                else if (float.TryParse(valueStr, out var f)) value = f;
                else value = valueStr; // fallback to string

                items.Add(new DataItem(name, value, GetDataItemType(value)));
            }
        }

        _logger.LogDebug("LoadFromFile completed for {Path}, items: {Count}", path, items.Count);
        return items;
    }

    private DataItemType GetDataItemType(object value)
        => value.GetType().Name switch
        {
            "Boolean" => DataItemType.Bool,
            "Int32" or "Int64" => DataItemType.Int,
            "Single" => DataItemType.Float,
            "String" => DataItemType.String,
            _ => throw new ArgumentException("Unsupported data type"),
        };

    public void Dispose()
    {
        _hardwareMonitor.FileChanged -= OnFileChanged;
        _dataForDisplaySubject?.Dispose();
        GC.SuppressFinalize(this);
        _logger.LogDebug("DataProvider disposed.");
    }
}
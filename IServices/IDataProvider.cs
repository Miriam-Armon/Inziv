using HardwareMonitor.Models;

namespace HardwareMonitor.IServices;

public interface IDataProvider : IDisposable
{
    IObservable<List<DataItem>> DataItemsStream { get; }
}

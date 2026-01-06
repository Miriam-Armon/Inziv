using System.Collections.ObjectModel;
using HardwareMonitor.Models;
using HardwareMonitor.IServices;
using System.Windows;

namespace HardwareMonitor.ViewModels;

public class MainWindowViewModel : IDisposable
{
    public ObservableCollection<DataItem> Items { get; set; } = new ObservableCollection<DataItem>();

    private readonly IDataProvider _provider;
    private readonly IDisposable _subscription;

    public MainWindowViewModel(IDataProvider provider)
    {
        _provider = provider;
        _subscription = _provider.DataItemsStream.Subscribe(newData =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var incoming in newData)
                {
                    var existing = Items.FirstOrDefault(i => i.Name == incoming.Name);

                    if (existing == null)
                    {
                        Items.Add(incoming);
                    }
                    else
                    {
                        existing.Value = incoming.Value;
                    }
                }
            });

        });
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _provider?.Dispose();
        GC.SuppressFinalize(this);
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HardwareMonitor.Models;

public class DataItem: INotifyPropertyChanged
{
    private static string _aggregatedText = string.Empty;
    public string Name { get; set; }

    private object _value; // bool, int, float, string

    public object Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }

    public DataItemType DataType { get; set; } 

    public DataItem(string name, object value, DataItemType type)
    {
        Name = name;
        DataType = type;
        if (DataType == DataItemType.String && value is string s)
        {
                _aggregatedText = string.IsNullOrEmpty(_aggregatedText)
                 ? s
                 : $"{_aggregatedText} {s}";
            _value = _aggregatedText;
        }
        else
        {
            _value = value;
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

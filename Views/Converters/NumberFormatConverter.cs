using HardwareMonitor.Models;
using System.Globalization;
using System.Windows.Data;

namespace HardwareMonitor.Views;

public class NumberFormatConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] == null || values[1] == null)
            return "";

        var value = values[0];
        if (value is null) return string.Empty;
        if (values[1] is DataItemType dit)
        switch (dit)
        {
            case DataItemType.Int:
                return value.ToString()?? string.Empty;

            case DataItemType.Float:
                if (value is double d)
                    return d.ToString("0.##");
                return value.ToString()?? string.Empty;

            default:
                return value.ToString()?? string.Empty;
        }
        return value;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

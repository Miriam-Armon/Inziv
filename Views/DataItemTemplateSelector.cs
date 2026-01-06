using HardwareMonitor.Models;
using System.Windows;
using System.Windows.Controls;

namespace HardwareMonitor.Views
{
    public class DataItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BoolTemplate { get; set; }
        public DataTemplate? NumberTemplate { get; set; }
        public DataTemplate? StringTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is DataItem dataItem)
            {
                return dataItem.DataType switch
                {
                    DataItemType.Bool => BoolTemplate,
                    DataItemType.Int or DataItemType.Float => NumberTemplate,
                    DataItemType.String => StringTemplate,
                    _ => base.SelectTemplate(item, container)
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}

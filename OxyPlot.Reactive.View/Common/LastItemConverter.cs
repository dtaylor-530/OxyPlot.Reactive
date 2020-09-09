using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OxyPlot.Reactive.View.Common
{
    internal class LastItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 3 && values[2] is int count && count > 0)
            {
                System.Windows.Controls.ItemsControl itemsControl = values[0] as System.Windows.Controls.ItemsControl;
                var itemContext = (values[1] as System.Windows.Controls.ContentPresenter).DataContext;

                var lastItem = itemsControl.Items[count - 1];

                return Equals(lastItem, itemContext);
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
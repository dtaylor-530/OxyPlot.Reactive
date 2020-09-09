using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OxyPlot.Reactive.View.Common
{
    internal class IndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length >= 2 && values[1] is UIElement uiElement && values[0] is ItemsControl itemsControl)
            {
                var index = 0;// Grid.GetColumn(uiElement);
                var row = Grid.GetRow(uiElement);
                HashSet<int> indexes = new HashSet<int>();
                for (int i = 0; i < itemsControl.Items.Count; i++)
                {
                    //if (i != index)
                    //{
                    var container = (UIElement)itemsControl.ItemContainerGenerator.ContainerFromIndex(i);

                    if (Grid.GetRow(container) == row)
                    {
                        int val = Grid.GetColumn(container);

                        while (!indexes.Add(val++))
                        {
                            Grid.SetColumn(container, val);
                        }
                    }
                }
                while (indexes.Contains(index)) index++;
                return index;
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
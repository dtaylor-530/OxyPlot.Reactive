using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OxyPlot.Reactive.DemoApp.Common
{
    public static class ObservableHelper
    {

        public static IObservable<T> SelectItemChanges<T>(this ComboBox comboBox)
        {
            var selectionChanged = comboBox.Events().SelectionChanged;

            // If using ComboBoxItems 
            var comboBoxItems = selectionChanged
          .SelectMany(a => a.AddedItems.OfType<ContentControl>())
          .StartWith(comboBox.SelectedItem as ContentControl)
          .Where(a => a != null)
          .Select(a => NewMethod2(a.Content))
            .Where(a => a.Equals(default(T)) == false);

            // If using type directly
            var directItems = selectionChanged
          .SelectMany(a => a.AddedItems.OfType<T>())
          .StartWith(NewMethod(comboBox.SelectedItem))
          .Where(a => a.Equals(default(T)) == false);

            // If using type indirectly
            var indirectItems = selectionChanged
          .SelectMany(a => a.AddedItems.Cast<object>().Select(a => TypeConvert.TryConvert<object, T>(a, out T t2) ? t2 : default))
          .StartWith(NewMethod2(comboBox.SelectedItem))
          .Where(a => a.Equals(default(T)) == false);

            var c = comboBoxItems.Amb(directItems).Amb(indirectItems);

            return c;

            static T NewMethod(object selectedItem)
            {
                return selectedItem is T t ? t : default;
            }

            static T NewMethod2(object selectedItem)
            {
                return TypeConvert.TryConvert(selectedItem, out T t2) ? t2 : default;
            }
        }


        public static IObservable<bool> SelectToggleChanges(this ToggleButton toggleButton, bool defaultValue = false)
        {
            return toggleButton.Events()
                .Checked.Select(a => true).Merge(toggleButton.Events()
                .Unchecked.Select(a => false))
                .StartWith(toggleButton.IsChecked ?? defaultValue);
        }
    }
}

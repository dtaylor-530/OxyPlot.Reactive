using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace OxyPlot.Reactive.DemoApp.Common
{
    public class AdjustColumnToItemCountBehavior : Behavior<Grid>
    {

        protected override void OnAttached()
        {
            AssociatedObject.LayoutUpdated += (sender, e) =>
            {

                for (int i = 0; i < AssociatedObject.Children.Count - AssociatedObject.ColumnDefinitions.Count; i++)
                {
                    AssociatedObject.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1, GridUnitType.Star) });
                }

                for (int i = 0; i <  AssociatedObject.ColumnDefinitions.Count - AssociatedObject.Children.Count; i++)
                {
                    AssociatedObject.ColumnDefinitions.RemoveAt(AssociatedObject.ColumnDefinitions.Count-1);
                }
            };
        }
    }
}

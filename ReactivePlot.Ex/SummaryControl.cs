using ReactivePlot.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ReactivePlot.Ex
{

    public class SummaryListBox : ListBox
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            if (item is SummaryControl)
            {
                return true;
            }
            return false;
        }
    }

    public class SummaryControl : ListBoxItem
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(SummaryControl), new PropertyMetadata(0.0));
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(string), typeof(SummaryControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IDoublePoint>), typeof(SummaryControl), new PropertyMetadata(null, ItemsSourceChanged));

        static SummaryControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SummaryControl), new FrameworkPropertyMetadata(typeof(SummaryControl)));
        }

        public SummaryControl()
        {
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public IEnumerable<IDoublePoint> ItemsSource
        {
            get { return (IEnumerable<IDoublePoint>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }


        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is SummaryControl summary && e.NewValue is IEnumerable<IDoublePoint<string>> enumerable)
            {
                summary.Value = enumerable.Sum(a => a.Value);
            }
        }

    }
}

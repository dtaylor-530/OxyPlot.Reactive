using NMT.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OxyPlot.Reactive.DemoApp.Common
{
    public class ProgressRingContentControl : ContentControl
    {
        private readonly Subject<bool> isBusyChanges = new Subject<bool>();
        private object content;

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(ProgressRingContentControl), new PropertyMetadata(false, Changed));

        private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ProgressRingContentControl).isBusyChanges.OnNext((bool)e.NewValue);
        }

        public ProgressRingContentControl()
        {
            var progressRing = new WindowsProgressRing { Foreground = Brushes.Gray, Width = 200, Height = 200, Speed = new Duration(TimeSpan.FromSeconds(2.5)), Items = 5 };
            isBusyChanges
                .StartWith(IsBusy)
                .DistinctUntilChanged()
                .Select(b => b ? progressRing : content ??= Content)
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Subscribe(a => Content = a);

        }
    }
}

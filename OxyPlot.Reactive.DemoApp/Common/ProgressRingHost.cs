using NMT.Wpf.Controls;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;

namespace OxyPlot.Reactive.DemoApp.Common
{
    public class ProgressRingHost : ContentControl
    {
        private readonly Subject<bool> isBusyChanges = new Subject<bool>();
        private WindowsProgressRing windowsProgressRing;
        private ContentPresenter contentPresenter;

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(ProgressRingHost), new PropertyMetadata(false, Changed));

        private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ProgressRingHost).isBusyChanges.OnNext((bool)e.NewValue);
        }

        public override void OnApplyTemplate()
        {
            windowsProgressRing = this.GetTemplateChild("PART_WindowsProgressRing") as WindowsProgressRing;
            contentPresenter = this.GetTemplateChild("PART_ContentPresenter") as ContentPresenter;

            base.OnApplyTemplate();
        }

        public ProgressRingHost()
        {
            isBusyChanges
                .StartWith(IsBusy)
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Subscribe(hideContent =>
                {
                    if (windowsProgressRing != null && contentPresenter != null)
                    {
                        windowsProgressRing.Visibility = !hideContent ? Visibility.Collapsed : Visibility.Visible;
                        contentPresenter.Visibility = hideContent ? Visibility.Collapsed : Visibility.Visible;
                    }
                });
        }
    }
}
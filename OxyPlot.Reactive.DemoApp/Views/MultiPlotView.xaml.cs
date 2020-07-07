
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using DynamicData;
using MoreLinq;
using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.Factory;
using ReactiveUI;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for MultiPlotView.xaml
    /// </summary>
    public partial class MultiPlotView : UserControl
    {
        private readonly ReadOnlyObservableCollection<PlotModel> plots;

        public MultiPlotView()
        {
            InitializeComponent();

            var abc = new string[] { "a", "b", "c" }.Repeat().GetEnumerator();

            var pacedObs = DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.6)).Select(a =>
            {
                abc.MoveNext();
                return KeyValuePair.Create(abc.Current, a);
            });

            var mplots = new MultiDateTimePlotModel<string, string>(scheduler: RxApp.MainThreadScheduler);
            pacedObs.Subscribe(mplots);

            mplots.ToObservableChangeSet()
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Bind(out plots)
                .Subscribe();

            ItemsControl1.ItemsSource = plots;

        }
    }
}

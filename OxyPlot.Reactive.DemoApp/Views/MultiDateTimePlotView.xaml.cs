
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using DynamicData;
using Endless;
using MoreLinq;
using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.Factory;
using OxyPlot.Reactive.Model;
using ReactiveUI;
namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for MultiPlotView.xaml
    /// </summary>
    public partial class MultiDateTimePlotView : UserControl
    {


        public MultiDateTimePlotView()
        {
            InitializeComponent();

            Standard();
            Accumulated();

        }
        private void Accumulated()
        {
            var pacedObs = DataSource.Observe1000PlusMinus().Take(200).Concat(DataSource.Observe1000PlusMinus().Skip(200).Pace(TimeSpan.FromSeconds(0.6))).Select(a =>
            {
                return KeyValuePair.Create(a.Key, DateTimePoint<string>.Create(a.Value.Key, a.Value.Value, a.Key + Enumerable.Range(1, 3).Random()));
            });

            var mplots = new MultiDateTimePlotAccumulatedModel<string, string>(scheduler: RxApp.MainThreadScheduler);
            pacedObs.Subscribe(mplots);
            mplots
               .Select((a, i) => (a, i))
           .ToObservableChangeSet(a => a.i)
               .ObserveOnDispatcher()
               .SubscribeOnDispatcher()
               .Transform(abc => new { model = abc.a, index = abc.i })
               .Sort(DynamicData.Binding.SortExpressionComparer<dynamic>.Descending(t => t.index))
               .Bind(out var plots)
               .Subscribe();

            ItemsControl2.ItemsSource = plots;
        }


        private void Standard()
        {
            var pacedObs = DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.6)).Select(a =>
            {
                return KeyValuePair.Create(a.Key, DateTimePoint<string>.Create(a.Value.Key, a.Value.Value, new string[] { "a", "b", "c" }.Random()));
            });

            var mplots = new MultiDateTimePlotModel<string, string>(scheduler: RxApp.MainThreadScheduler);
            pacedObs.Subscribe(mplots);

            _ = mplots.ToObservableChangeSet()
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Bind(out var plots)
                .Subscribe();

            ItemsControl1.ItemsSource = plots;
        }
    }
}

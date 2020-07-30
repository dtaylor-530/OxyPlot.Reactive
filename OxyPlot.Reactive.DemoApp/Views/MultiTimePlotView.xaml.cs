
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
using OxyPlot.Reactive.Multi;
using OxyPlot.Wpf;
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

            ItemsControlAccumulated.ItemsSource = AccumulatedGroup();
            ItemsControlStandard.ItemsSource = Standard();
            ItemsControlGroup.ItemsSource = AccumulatedGroup();
        }
        private IReadOnlyCollection<dynamic> Accumulated()
        {
            var pacedObs = TimeDataSource.Observe1000PlusMinus().Take(200).Concat(TimeDataSource.Observe1000PlusMinus().Skip(200).Pace(TimeSpan.FromSeconds(0.6))).Select(a =>
            {
                return KeyValuePair.Create(a.Key, TimePoint<string>.Create(a.Value.Key, a.Value.Value, a.Key + Enumerable.Range(1, 3).Random()));
            });

            var mplots = new MultiTimePlotAccumulatedModel<string, string>(scheduler: RxApp.MainThreadScheduler);
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

            return plots;
        }


        private IReadOnlyCollection<dynamic> AccumulatedGroup()
        {
            var pacedObs = TimeDataSource.Observe1000().Take(200).Concat(TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.6))).Select(a =>
            {
                return KeyValuePair.Create(a.Key, TimePoint<string>.Create(a.Value.Key, a.Value.Value, a.Key + Enumerable.Range(1, 3).Random()));
            });

            var mplots = new TimePlotGroupAccumulatedModel<string, string>(scheduler: RxApp.MainThreadScheduler);
            TimeView1.TimeSpanObservable.Subscribe(mplots);
            Observable.FromEventPattern(ComboBox1, nameof(ComboBox.SelectionChanged))
                .SelectMany(a=>(a.EventArgs as SelectionChangedEventArgs).AddedItems.Cast<Operation>())
                .Subscribe(mplots);

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

            return plots;
        }


        private IReadOnlyCollection<KeyValuePair<string,PlotModel>> Standard()
        {
            var pacedObs = TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.6)).Select(a =>
            {
                return KeyValuePair.Create(a.Key, TimePoint<string>.Create(a.Value.Key, a.Value.Value, new string[] { "a", "b", "c" }.Random()));
            });

            var mplots = new MultiTimePlotModel<string, string>(scheduler: RxApp.MainThreadScheduler);
            pacedObs.Subscribe(mplots);

            _ = mplots.ToObservableChangeSet()
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Bind(out var plots)
                .Subscribe();

            return plots;
     
        }


    }
}

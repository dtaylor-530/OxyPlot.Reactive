using DynamicData;
using Endless;
using MoreLinq;
using OxyPlot.Axes;
using OxyPlot.Data.Factory;
using OxyPlot.Data.Common;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Multi;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

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

            ItemsControlAccumulated.ItemsSource = Accumulated();
            ItemsControlStandard.ItemsSource = Standard();
            ItemsControlGroup.ItemsSource = AccumulatedGroup();
        }

        private IReadOnlyCollection<dynamic> Accumulated()
        {
            var pacedObs = TimeDataSource.Observe1000PlusMinus().Take(200).Concat(TimeDataSource.Observe1000PlusMinus().Skip(200).Pace(TimeSpan.FromSeconds(0.6))).Select(a =>
            {
                return KeyValuePair.Create(a.Key, (ITimePoint<string>)new TimeDemoPoint(a.Key + Enumerable.Range(1, 3).Random(), a.Value.Key, a.Value.Value));
            });

            var mplots = new MultiTimePlotAccumulatedModel();
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
                return KeyValuePair.Create(a.Key, (ITimePoint<string>)new TimeDemoPoint(new string[] { "a", "b", "c" }.Random(), a.Value.Key, a.Value.Value));
            });

            var mplots = new TimePlotGroupAccumulatedModel<string, string>(scheduler: RxApp.MainThreadScheduler);
            TimeView1.TimeSpanObservable.Subscribe(mplots);
            Observable.FromEventPattern(ComboBox1, nameof(ComboBox.SelectionChanged))
                .SelectMany(a => (a.EventArgs as SelectionChangedEventArgs).AddedItems.Cast<Operation>())
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

        private IReadOnlyCollection<KeyValuePair<string, PlotModel>> Standard()
        {
            var pacedObs = TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.6)).Select(a =>
            {
                return KeyValuePair.Create(a.Key, (ITimePoint<string>)new TimeDemoPoint(new string[] { "a", "b", "c" }.Random(), a.Value.Key, a.Value.Value));
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


        public class MultiTimePlotAccumulatedModel : MultiTimePlotAccumulatedModel<string, string>
        {
            protected override ITimePoint<string> CreatePoint(ITimePoint<string> xy0, ITimePoint<string> xy)
            {
                return new TimeDemoPoint(xy.Key, xy.Var, (xy0?.Value??0) + xy.Value);
            }

        }


        public class TimeDemoPoint : ITimePoint<string>
        {
            public TimeDemoPoint(string key, DateTime dateTime, double value) 
            {
                Key = key;
                Var = dateTime;
                Value = value;
                Orientation = new [] { Orientation.Horizontal, Orientation.Vertical }.Random();
            }

            public string Key { get; }

            public DateTime Var { get; }

            public double Value { get; }
            public Orientation Orientation { get; }

            public DataPoint GetDataPoint()
            {
                return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
            }

            public override string ToString()
            {
                return $"{Var:F}, {Value}, {Key}";
            }

        }
    }
}
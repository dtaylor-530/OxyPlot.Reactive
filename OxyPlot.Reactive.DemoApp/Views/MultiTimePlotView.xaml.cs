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
using OxyPlot.Reactive.DemoApp.Model;
using OnTheFlyStats;
using OxyPlot.Reactive.Model.Enum;
using OxyPlot.Reactive.DemoApp.Common;

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
            ItemsControlAccumulatedGroup.ItemsSource = AccumulatedGroup();
            ItemsControlValueKey.ItemsSource = ValueKey();
            ItemsControlRollingOperation.ItemsSource = RollingOperation();
        }

        private IReadOnlyCollection<dynamic> Accumulated()
        {
            var pacedObs = TimeDataSource.Observe1000PlusMinus().Take(200).Concat(TimeDataSource.Observe1000PlusMinus().Skip(200).Pace(TimeSpan.FromSeconds(0.6))).Select(a =>
            {
                var r = a.Key + Enumerable.Range(1, 3).Random();
                return KeyValuePair.Create(a.Key, (ITimeGroupPoint<string, string>)new TimeDemoPoint(r, r, a.Value.Key, a.Value.Value));
            });

            var mplots = new MultiTimePlotAccumulatedDemoModel();
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
                var r = a.Key + Enumerable.Range(1, 3).Random();
                return KeyValuePair.Create(a.Key, (ITimeGroupPoint<string, string>)new TimeDemoPoint(r, r, a.Value.Key, a.Value.Value));
            });

            var mplots = new MultiTimePlotGroupAccumulatedModel<string, string>(scheduler: RxApp.MainThreadScheduler);

            pacedObs.Subscribe(a => mplots.OnNext(a));

            TimeView1.TimeSpanObservable.Subscribe(mplots);


            Observable.FromEventPattern(ComboBox1, nameof(ComboBox.SelectionChanged))
                .SelectMany(a => (a.EventArgs as SelectionChangedEventArgs).AddedItems.Cast<Operation>())
                .Subscribe(mplots);

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
                return KeyValuePair.Create(a.Key, (ITimeGroupPoint<string, string>)new TimeDemoPoint(a.Key, a.Key, a.Value.Key, a.Value.Value));
            });

            var mplots = new MultiTimePlotAccumulatedModel<string, string>(scheduler: RxApp.MainThreadScheduler);
            // pacedObs.SubscribeX<string,string,TimeGroupPoint<string,string>>(mplots);
            pacedObs.Subscribe(a => mplots.OnNext(a));

            _ = mplots.ToObservableChangeSet()
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Bind(out var plots)
                .Subscribe();

            return plots;
        }


        private IReadOnlyCollection<KeyValuePair<string, PlotModel>> RollingOperation()
        {
            Random random = new Random();
            var pacedObs = TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.1)).Select(a =>
            {
                var r = a.Key + Enumerable.Range(1, 3).Random();
                return KeyValuePair.Create(a.Key, (ITimeStatsGroupPoint<string, string>)new TimeDemoStringPoint(r, r, a.Value.Key, a.Value.Value));
            });

            var mplots = new MultiTimePlotGroupStatsModel<string, string>(scheduler: RxApp.MainThreadScheduler);

            _ = ComboBox1.SelectItemChanges<Operation>().Subscribe(op => mplots.OnNext(op));

            _ = ComboBox2.SelectItemChanges<RollingOperation>().Subscribe(op => mplots.OnNext(op));

            TimeView1.TimeSpanObservable.Subscribe(mplots);

            pacedObs.Subscribe(a => mplots.OnNext(a));

            _ = mplots.ToObservableChangeSet()
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Bind(out var plots)
                .Subscribe();

            return plots;
        }


        private IReadOnlyCollection<KeyValuePair<string, PlotModel>> ValueKey()
        {
            Random random = new Random();

            var pacedObs = TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.6)).Select(a =>
            {
                var r = random.NextDouble() * 100;
                return KeyValuePair.Create(a.Key, (ITimeStatsGroupPoint<string, double>)new TimeDemoDoublePoint(a.Key, r, a.Value.Key, a.Value.Value));
            });

            var mplots = new MultiTimePlotKeyValueGroupStatsModel(scheduler: RxApp.MainThreadScheduler);

            PowerComboBox.SelectItemChanges<int>().Subscribe(op=>
            {
                mplots.OnNext(op);
            });

            ComboBox2.SelectItemChanges<RollingOperation>().Subscribe(op =>
            {
                mplots.OnNext(op);
            });

            pacedObs.Subscribe(a => mplots.OnNext(a));

            _ = mplots.ToObservableChangeSet()
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Bind(out var plots)
                .Subscribe();

            return plots;
        }

        public class MultiTimePlotAccumulatedDemoModel : MultiTimePlotAccumulatedModel<string, string>
        {
            protected override ITimeGroupPoint<string, string> CreatePoint(ITimeGroupPoint<string, string> xy0, ITimeGroupPoint<string, string> xy)
            {
                return new TimeDemoPoint(xy.GroupKey, xy.Key, xy.Var, (xy0?.Value ?? 0) + xy.Value);
            }

        }


        public class TimeDemoDoublePoint : ITimeStatsGroupPoint<string, double>
        {
            public TimeDemoDoublePoint(string groupKey, double key, DateTime dateTime, double value)
            {
                GroupKey = groupKey;
                Key = key;
                Var = dateTime;
                Value = value;
                Orientation = new[] { Orientation.Horizontal, Orientation.Vertical }.Random();
            }

            public string GroupKey { get; }

            public double Key { get; }

            public DateTime Var { get; }

            public double Value { get; }

            // Used in demo view
            public Orientation Orientation { get; }

            public Stats Model { get; }

            public DataPoint GetDataPoint()
            {
                return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
            }

            public override string ToString()
            {
                return $"{Var:F}, {Value}, {Key}";
            }

        }


        public class TimeDemoStringPoint : ITimeStatsGroupPoint<string, string>
        {
            public TimeDemoStringPoint(string groupKey, string key, DateTime dateTime, double value)
            {
                GroupKey = groupKey;
                Key = key;
                Var = dateTime;
                Value = value;
                //Orientation = new[] { Orientation.Horizontal, Orientation.Vertical }.Random();
            }

            public string GroupKey { get; }

            public string Key { get; }

            public DateTime Var { get; }

            public double Value { get; }

            // public Orientation Orientation { get; }

            public Stats Model { get; }

            public DataPoint GetDataPoint()
            {
                return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
            }

            public override string ToString()
            {
                return $"{Var:F}, {Value}, {Key}";
            }

        }

        public class TimeDemoPoint : ITimeGroupPoint<string, string>
        {
            public TimeDemoPoint(string groupKey, string key, DateTime dateTime, double value)
            {
                GroupKey = groupKey;
                Key = key;
                Var = dateTime;
                Value = value;
                Orientation = new[] { Orientation.Horizontal, Orientation.Vertical }.Random();
            }

            public string GroupKey { get; }

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
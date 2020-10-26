using DynamicData;
using DynamicData.Binding;
using MoreLinq;
using OxyPlot;
using OxyPlot.Reactive;
using OxyPlot.Data.Common;
using OxyPlot.Data.Factory;
using OxyPlot.Reactive.DemoApp.ViewModels;
using OxyPlot.Reactive.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for LineView.xaml
    /// </summary>
    public partial class MultiDateTimeGroupView : UserControl
    {
        public MultiDateTimeGroupView()
        {
            InitializeComponent();

            var pacedObs = TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.3));

            var model1 = new TimeGroupModel<string>(PlotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.SubscribeCustom(model1);

            var model2 = new TimeGroup2Model<string>(PlotView2.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.SubscribeCustom(model2);

            var model4= new TimeGroupOnTheFlyStatsModel<string>(PlotView2.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            IDisposable disposable = pacedObs.SubscribeCustom4(model4);

            (model2 as IObservable<IChangeSet<ITimeRangePoint<string>>>)
                .Sort(SortExpressionComparer<ITimeRangePoint<string>>.Descending(t => t.Range.Max))
                .Top(4)
                .Bind(out var collection2)
                .Subscribe();

            DataGrid2.ItemsSource = collection2;

            (model2 as IObservable<ITimePoint<string>>).Subscribe(p =>
            {
                var n = collection2.Index().SingleOrDefault(a => (a.Value.Key, a.Value.Var) == (p.Key, p.Var)).Key;
                DataGrid2.SelectedIndex = n;
                DataGrid2.ScrollIntoView(DataGrid2.Items[n]);
            });

            var model3 = new CustomTimeGroup2Model<string>(PlotView3.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.SubscribeCustom(model3);

            TimeView1.TimeSpanObservable.Subscribe(x =>
            {
                model1?.OnNext(x);
                model2?.OnNext(x);
                model3?.OnNext(x);
            });

            ComboBox1.SelectionChanged += (s, e) =>
            {
                var op = e.AddedItems.Cast<Operation>().Single();
                model1.OnNext(op);
                model2.OnNext(op);
            };
        }

        public class GroupViewModel<T, R, S> : ReactiveObject
        {
            private readonly ObservableAsPropertyHelper<IReadOnlyCollection<T>> collection;

            public GroupViewModel(R key, IObservable<IChangeSet<T, S>> observable)
            {
                collection = observable.ToCollection().ToProperty(this, a => a.Collection);
                Key = key;
            }

            public R Key { get; }

            public IReadOnlyCollection<T> Collection => collection.Value;
        }
    }
}
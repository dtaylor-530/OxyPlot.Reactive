using DynamicData;
using DynamicData.Binding;
using MoreLinq;
using OxyPlot;
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
using OxyPlot.Reactive.DemoApp.Model;
using OxyPlot.Reactive.Model.Enum;

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

            var model1 = new TimeGroupDemoModel<string>(PlotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.SubscribeCustom(model1);


            //----------------------------------------------

            var model2 = new TimeGroupModel<string>(PlotView2.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.SubscribeCustom(model2);

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


            //----------------------------------------------
            var model3 = new TimeGroupOnTheFlyStatsModel<string>(PlotView3.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            IDisposable disposable = pacedObs.SubscribeCustom4(model3);


            //----------------------------------------------


            var model4 = new CustomTimeGroup2Model<string>(PlotView4.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.SubscribeCustom(model4);


            //----------------------------------------------


            TimeView1.TimeSpanObservable.Subscribe(x =>
            {
                model1?.OnNext(x);
                model2?.OnNext(x);
                model3?.OnNext(x);
                model4?.OnNext(x);
            });

            ComboBox1.SelectionChanged += (s, e) =>
            {
                var op = e.AddedItems.Cast<Operation>().Single();
                model1.OnNext(op);
                model2.OnNext(op);
                model3.OnNext(op);
                model4.OnNext(op);
            };

            ComboBox2.SelectionChanged += (s, e) =>
            {
                var op = e.AddedItems.Cast<RollingOperation>().Single();
                model3.OnNext(op);
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
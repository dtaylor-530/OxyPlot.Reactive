using DynamicData;
using DynamicData.Binding;
using MoreLinq;
using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.Model;
using OxyPlot.Reactive.DemoApp.ViewModels;
using ReactivePlot.Data.Factory;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using ReactivePlot.Data.Common;
using ReactivePlot.OxyPlot;
using ReactivePlot.Common;
using ReactivePlot.OxyPlot.PlotModel;
using ReactivePlot.Time;

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

            var timePlotModel = new OxyTimePlotModel<string, ITimeRangePoint<string>>(PlotView2.Model ??= new PlotModel());
            var model2 = new TimeGroupModel<string>(timePlotModel, scheduler: RxApp.MainThreadScheduler);

            pacedObs.SubscribeCustom(model2);

            (model2 as IObservable<IChangeSet<ITimeRangePoint<string>>>)
    .Sort(SortExpressionComparer<ITimeRangePoint<string>>.Descending(t => t.Range.Max))
    .Top(4)
    .Bind(out var collection2)
    .Subscribe();

            DataGrid2.ItemsSource = collection2;


            (timePlotModel as IObservable<ITimePoint<string>>).Subscribe(p =>
            {
                var n = collection2.Index().SingleOrDefault(a => (a.Value.Key, a.Value.Var) == (p.Key, p.Var)).Key;
                DataGrid2.SelectedIndex = n;
                DataGrid2.ScrollIntoView(DataGrid2.Items[n]);
            });


            //----------------------------------------------
            var model3 = new OxyTimeGroupOnTheFlyStatsModel<string>(PlotView3.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

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

            ComboBox1.SelectItemChanges<Operation>().Subscribe(op =>
            {
                model1.OnNext(op);
                model2.OnNext(op);
                model3.OnNext(op);
                model4.OnNext(op);
            });

            ComboBox2.SelectItemChanges<RollingOperation>().Subscribe(op =>
            {
                model3.OnNext(op);
            });
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
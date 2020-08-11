using DynamicData;
using Itenso.TimePeriod;
using LinqStatistics;
using MoreLinq;
using OxyPlot.Reactive;
using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.Factory;
using OxyPlot.Reactive.DemoApp.ViewModels;
using OxyPlot.Reactive.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace OxyPlotEx.DemoAppCore.Pages
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



            var model1 = new TimeGroupModel<string>(PlotView1.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.Subscribe(model1);

            (model1 as
                   IObservable<IChangeSet<ITimeRangePoint<string>>>)
                 .Bind(out var collection1)
                                 .ObserveOnDispatcher()
                    .SubscribeOnDispatcher()
                 .Subscribe(a =>
                 {
                     var coll = collection1;
                 });

            DataGrid1.ItemsSource = collection1;

            var model2 = new TimeGroup2Model<string>(PlotView2.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.Subscribe(model2);

            (model2 as IObservable<Range<DateTime>[]>)
                .CombineLatest(pacedObs, (a, b) => (a.FirstOrDefault(c => c.Min <= b.Value.Key && c.Max >= b.Value.Key), b))
                .Where(c => c.Item1 != null)
                .ToObservableChangeSet(a => Guid.NewGuid())
                .Group(a => a.Item1)
                .Transform(a => new GroupViewModel<object, Range<DateTime>, Guid>(a.Key, a.Cache.Connect().Transform(a => (object)new { a.b.Key, date = a.b.Value.Key, a.b.Value.Value })))
                .ObserveOnDispatcher()
                  .SubscribeOnDispatcher()
                .Bind(out var rangeCollection)
                .Subscribe();

            DataGrid2.ItemsSource = rangeCollection;

            (model2 as IObservable<ITimePoint<string>>).Subscribe(p =>
            {
                var n = rangeCollection.Select((a, i) => (key: a.Key.Min, i)).SingleOrDefault(a => a.key == p.Var).i;
                DataGrid2.SelectedIndex = n;
                DataGrid2.ScrollIntoView(DataGrid2.Items[n]);
            });




            CustomMultiDateTimeGroup2Model<string> model3 = new CustomMultiDateTimeGroup2Model<string>(PlotView3.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            pacedObs.Subscribe(model3);

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

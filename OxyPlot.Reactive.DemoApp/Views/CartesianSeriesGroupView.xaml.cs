using DynamicData;
using LinqStatistics;
using MoreLinq;
using OxyPlot.Reactive.DemoApp.Common;
using ReactivePlot.Cartesian;
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
using ReactivePlot.Common;
using ReactivePlot.OxyPlot;
using ReactivePlot.OxyPlot.PlotModel;

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for LineView.xaml
    /// </summary>
    public partial class CartesianSeriesGroupView : UserControl
    {
        public CartesianSeriesGroupView()
        {
            InitializeComponent();

            var pacedObs = DataSource.Observe1000().ToDoubles().Pace(TimeSpan.FromSeconds(0.3));
            var plotModel = new OxyCartesianPlotModel<string, IDoubleRangePoint<string>>(PlotView2.Model ??= new OxyPlot.PlotModel());
            var model2 = new CartesianGroupModel<string>(plotModel, scheduler: RxApp.MainThreadScheduler);

         
            pacedObs.SubscribeCustom(model2);

            (model2 as IObservable<(double size, Range<double>[] ranges)>)
                .CombineLatest(pacedObs, (a, b) => (a.ranges.FirstOrDefault(c => c.Min <= b.Value.Key && c.Max >= b.Value.Key), b))
                .Where(c => c.Item1 != null)
                .ToObservableChangeSet(a => Guid.NewGuid(), limitSizeTo: 10)
                .Group(a => a.Item1)
                .Transform(a => new GroupViewModel<object, Range<double>, Guid>(a.Key, a.Cache.Connect().Transform(a => (object)new { a.b.Key, date = a.b.Value.Key, a.b.Value.Value })))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Bind(out var rangeCollection)
                .Subscribe();

            DataGrid2.ItemsSource = rangeCollection;


            plotModel.Subscribe(p =>
            {
                var n = rangeCollection.Select((a, i) => (key: a.Key.Min, i)).SingleOrDefault(a => a.key == p.Var).i;
                DataGrid2.SelectedIndex = n;
                DataGrid2.ScrollIntoView(DataGrid2.Items[n]);
            });

            _ = NumbersBox.SelectItemChanges<int>().Subscribe(op => model2.OnNext(op));
            _ = ComboBox1.SelectItemChanges<Operation>().Subscribe(op => model2.OnNext(op));
        }

        private void CartesianSeriesGroupView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void NumbersBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
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
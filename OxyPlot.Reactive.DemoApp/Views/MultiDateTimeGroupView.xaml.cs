using DynamicData;
using Exceptionless.DateTimeExtensions;
using MoreLinq;
using OxyPlot.Reactive;
using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.Factory;
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
        MultiDateTimeGroupModel<string> model;
        MultiDateTimeGroup2Model<string> model2;
        public MultiDateTimeGroupView()
        {
            InitializeComponent();
            var pacedObs = DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.3));


            model = new MultiDateTimeGroupModel<string>(PlotView1.Model ??= new OxyPlot.PlotModel(), scheduler: ReactiveUI.RxApp.MainThreadScheduler);
            model2 = new MultiDateTimeGroup2Model<string>(PlotView2.Model ??= new OxyPlot.PlotModel(), scheduler: ReactiveUI.RxApp.MainThreadScheduler);

            pacedObs.Subscribe(model);
            pacedObs.Subscribe(model2);

            (model2 as IObservable<DateTimeRange[]>)
                .CombineLatest(pacedObs, (a, b) => (a.SingleOrDefault(c => c.Start <= b.Value.Key && c.End >= b.Value.Key), b))
                .Where(c => c.Item1 != null)
                .ToObservableChangeSet(a => Guid.NewGuid())
                .Group(a => a.Item1)
                .Transform(a => new GroupViewModel<object, DateTimeRange, Guid>(a.Key, a.Cache.Connect().Transform(a => (object)new { a.b.Key, date = a.b.Value.Key, a.b.Value.Value })))
                .ObserveOnDispatcher()
                  .SubscribeOnDispatcher()
                .Bind(out var rangeCollection)
                .Subscribe();

            DataGridRange.ItemsSource = rangeCollection;

            (model2 as IObservable<IDateTimeKeyPoint<string>>).Subscribe(p =>
            {
                var n = rangeCollection.Select((a, i) => (key:a.Key.Start/* + (a.Key.End-a.Key.Start)/2*/, i)).SingleOrDefault(a => a.key == p.DateTime).i;
                DataGridRange.SelectedIndex = n;
                DataGridRange.ScrollIntoView(DataGridRange.Items[n]);
            });
        }

        private void IntervalBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = TimeUnit.Parse(NumbersBox.SelectedItem.ToString() + ((IntervalBox?.SelectedItem.ToString())?.First().ToString().ToLower() ?? "s"));
            model?.OnNext(x);
            model2?.OnNext(x);
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

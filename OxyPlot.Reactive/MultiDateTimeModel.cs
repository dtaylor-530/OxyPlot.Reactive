#nullable enable

using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using OxyPlot.Series;
using MoreLinq;
using System.Reactive.Threading.Tasks;
using OxyPlotEx.Common;

namespace OxyPlotEx.ViewModel
{
    public class MultiDateTimeModel<T> : MultiPlotModel<T, DateTime>, IObservable<IDateTimeKeyPoint<T>>, IObserver<int>, IObserver<IDateTimeKeyPoint<T>>
    {
        readonly Subject<IDateTimeKeyPoint<T>> subject = new Subject<IDateTimeKeyPoint<T>>();
        private int? count;

        public MultiDateTimeModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T>? comparer=null) : base(dispatcher, model, comparer)
        {
        }

        protected override void ModifyPlotModel()
        {
            plotModel.Axes.Add(new DateTimeAxis());
        }

        protected override void Refresh(IList<Unit> units)
        {
            this.dispatcher.BeginInvoke(async () =>
            {
                foreach (var keyValue in DataPoints.ToArray())
                {
                    _ = await Task.Run(() =>
                      {
                          lock (lck)
                          {
                              return count.HasValue ?
                               Enumerable.TakeLast(ToDataPoints2(keyValue), count.Value).ToArray() :
                               ToDataPoints2(keyValue).ToArray();
                          }
                      }).ContinueWith(async points =>
                      {
                          AddToSeries(await points, keyValue.Key.ToString());
                      }, TaskScheduler.FromCurrentSynchronizationContext());
                }

                if (showAll)
                {
                    _ = await Task.Run(() =>
                    {
                        lock (lck)
                        {
                            var xx = ToDataPoints(DataPoints.SelectMany(a => a.Value.Select(c => KeyValuePair.Create(a.Key, c))));

                            return count.HasValue ?
                            Enumerable.TakeLast(xx, count.Value).ToArray() :
                            xx.ToArray();
                        }

                    }).ContinueWith(async points =>
                    {
                        AddToSeries((await points), "All");
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                plotModel.InvalidatePlot(true);
            });


            IEnumerable<IDateTimeKeyPoint<T>> ToDataPoints2(KeyValuePair<T, List<(DateTime X, double Y)>> keyValue)
                => ToDataPoints(keyValue.Value.Select(c => KeyValuePair.Create(keyValue.Key, c)));

        }


        IEnumerable<IDateTimeKeyPoint<T>> ToDataPoints(IEnumerable<KeyValuePair<T, (DateTime X, double Y)>> collection)
            => collection
            .OrderBy(c => c.Value.X)
            .Scan(new DateTimePoint<T>(), (xy0, xy) => new DateTimePoint<T>(xy.Value.X, Combine(xy0.Value, xy.Value.Y), xy.Key))
            .Cast<IDateTimeKeyPoint<T>>()
            .Skip(1);

        protected virtual void AddToSeries(IDateTimeKeyPoint<T>[] items, string title)
        {
            if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
            {
                series = (OxyFactory.Build(items, title));

                series.ToMouseDownEvents().Subscribe(e =>
                {
                    var time = DateTimeAxis.ToDateTime(series.InverseTransform(e.Position).X);
                    var point = items.MinBy(a => Math.Abs((a.DateTime - time).Ticks)).First();
                    subject.OnNext(point);
                });

                plotModel.Series.Add(series);
            }

            series.ItemsSource = items;
        }



        public IDisposable Subscribe(IObserver<IDateTimeKeyPoint<T>> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(int count)
        {
            this.count = count;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(IDateTimeKeyPoint<T> item)
        {
            Task.Run(() => AddToDataPoints(KeyValuePair.Create(item.Key, (item.DateTime, item.Value))))
                .ToObservable()
                .Subscribe(refreshSubject.OnNext);
        }

    }
}

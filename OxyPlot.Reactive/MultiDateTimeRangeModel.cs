#nullable enable

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
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System.Reactive.Concurrency;
using Exceptionless.DateTimeExtensions;

namespace OxyPlot.Reactive
{
    public class MultiDateTimeRangeModel<TKey> : MultiPlotModel<TKey, DateTime>, IObservable<IDateTimeKeyPoint<TKey>>, IObserver<int>, IObserver<IDateTimeKeyPoint<TKey>>, IObserver<TimeSpan>
    {
        readonly Subject<IDateTimeKeyPoint<TKey>> subject = new Subject<IDateTimeKeyPoint<TKey>>();
        private RangeType rangeType = RangeType.None;
        private int count;
        private DateTimeRange? dateTimeRange;
        private TimeSpan? timeSpan;

        public MultiDateTimeRangeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override void ModifyPlotModel()
        {
            plotModel.Axes.Add(new DateTimeAxis());
        }

        protected override void Refresh(IList<Unit> units)
        {
            scheduler.Schedule(async () =>
            {
                KeyValuePair<TKey, List<(DateTime, double)>>[]? arr = default;
                lock (DataPoints)
                    arr = DataPoints.ToArray();

                foreach (var keyValue in arr)
                {
                    _ = await Task.Run(() =>
                      {
                          return Switch(keyValue.Value.ToArray().Select(c => KeyValuePair.Create(keyValue.Key, c))).ToArray();

                      }).ContinueWith(async points =>
                      {
                          AddToSeries(await points, keyValue.Key.ToString());
                      });
                }

                if (showAll)
                {
                    _ = await Task.Run(() =>
                    {
                        lock (DataPoints)
                        {
                            return Switch(arr.SelectMany(a => a.Value.Select(c => KeyValuePair.Create(a.Key, c)))).ToArray();
                        }

                    }).ContinueWith(async points =>
                    {
                        AddToSeries(await points, "All");
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                lock (plotModel)
                    plotModel.InvalidatePlot(true);
            });

            IEnumerable<IDateTimeKeyPoint<TKey>> Switch(IEnumerable<KeyValuePair<TKey, (DateTime X, double Y)>> col)
            {
                return rangeType switch
                {
                    RangeType.None => ToDataPoints(col),
                    RangeType.Count => Enumerable.TakeLast(ToDataPoints(col), count),
                    RangeType.TimeSpan when timeSpan.HasValue => ToDataPoints(col.ToArray().Filter(timeSpan.Value, a => a.Value.X)),
                    RangeType.DateTimeRange when dateTimeRange != null => ToDataPoints(col.Filter(dateTimeRange, a => a.Value.X)),
                    _ => throw new ArgumentOutOfRangeException("fdssffd")
                };


                IEnumerable<IDateTimeKeyPoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, (DateTime X, double Y)>> collection)
                    => collection
                    .OrderBy(c => c.Value.X)
                    .Scan(new DateTimePoint<TKey>(), (xy0, xy) => new DateTimePoint<TKey>(xy.Value.X, Combine(xy0.Value, xy.Value.Y), xy.Key))
                    .Cast<IDateTimeKeyPoint<TKey>>()
                    .Skip(1);

            }
        }




        protected virtual void AddToSeries(IDateTimeKeyPoint<TKey>[] items, string title)
        {
            lock (plotModel)
            {
                if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
                {
                    series = OxyFactory.Build(items, title);

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
        }

        public IDisposable Subscribe(IObserver<IDateTimeKeyPoint<TKey>> observer)
        {
            return subject.Subscribe(observer);
        }


        public void OnNext(IDateTimeKeyPoint<TKey> item)
        {
            Task.Run(() => AddToDataPoints(KeyValuePair.Create(item.Key, (item.DateTime, item.Value))))
                .ToObservable()
                .Subscribe(refreshSubject.OnNext);
        }


        public void OnNext(int count)
        {
            this.count = count;
            rangeType = RangeType.Count;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            rangeType = RangeType.TimeSpan;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(DateTimeRange value)
        {
            dateTimeRange = value;
            rangeType = RangeType.DateTimeRange;
            refreshSubject.OnNext(Unit.Default);
        }

        enum RangeType
        {
            None,
            Count = 1,
            TimeSpan,
            DateTimeRange
        }
    }
}

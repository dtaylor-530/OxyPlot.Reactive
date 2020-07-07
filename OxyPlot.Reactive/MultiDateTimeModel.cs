﻿#nullable enable

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

namespace OxyPlot.Reactive
{
    public class MultiDateTimeModel<TKey> : MultiPlotModel<TKey, DateTime>, IObservable<IDateTimeKeyPoint<TKey>>, IObserver<int>, IObserver<IDateTimeKeyPoint<TKey>>
    {
        readonly Subject<IDateTimeKeyPoint<TKey>> subject = new Subject<IDateTimeKeyPoint<TKey>>();
        protected int? count;
        protected DateTime min=DateTime.MaxValue, max = DateTime.MinValue;
        public MultiDateTimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override void ModifyPlotModel()
        {
            plotModel.Axes.Add(new DateTimeAxis());
        }

        protected override void Refresh(IList<Unit> units)
        {
            _ = (this as IMixedScheduler).ScheduleAction(async () =>
              {
                  KeyValuePair<TKey, ICollection<KeyValuePair<DateTime, double>>>[]? dataPoints;
                  lock (DataPoints)
                      dataPoints = DataPoints.ToArray();


                  foreach (var keyValue in dataPoints)
                  {
                      _ = await Task.Run(() =>
                        {
                            lock (lck)
                            {
                                var ssx = ToDataPoints(Flatten(keyValue)).ToArray();
                                return count.HasValue ?
                                 Enumerable.TakeLast(ToDataPoints(Flatten(keyValue)), count.Value).ToArray() :
                                 ToDataPoints(Flatten(keyValue)).ToArray();
                            }
                        }).ContinueWith(async points => AddToSeries(await points, keyValue.Key.ToString()));
                  }

                  if (showAll)
                  {
                      _ = await Task.Run(() =>
                        {
                            lock (lck)
                            {
                                var xx = ToDataPoints(DataPoints.SelectMany(a => Flatten(a))).ToArray();
                                return count.HasValue ? Enumerable.TakeLast(xx, count.Value).ToArray() : xx;
                            }

                        }).ContinueWith(async points => AddToSeries(await points, "All"));
                  }
                  plotModel.InvalidatePlot(true);
              });


            IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime,double>>> Flatten(KeyValuePair<TKey, ICollection<KeyValuePair<DateTime, double>>> keyValue)
                => keyValue.Value.ToArray().Select(c => KeyValuePair.Create(keyValue.Key, c));
        }

        protected virtual IEnumerable<IDateTimeKeyPoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
            => collection
            .Scan(new DateTimePoint<TKey>(), (xy0, xy) => new DateTimePoint<TKey>(xy.Value.Key, Combine(xy0.Value, xy.Value.Value), xy.Key))
            .Cast<IDateTimeKeyPoint<TKey>>()
            .Skip(1);

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

        protected override void AddToDataPoints(KeyValuePair<TKey, KeyValuePair<DateTime, double>> item)
        {
            try
            {
                var newdp = item.Value;
                min = new DateTime(Math.Min(newdp.Key.Ticks, min.Ticks));
                max = new DateTime(Math.Max(newdp.Key.Ticks, max.Ticks));

                lock (DataPoints)
                {
                    if (!DataPoints.ContainsKey(item.Key))
                        DataPoints[item.Key] = new SortedList<DateTime, double>();
                    DataPoints[item.Key].Add(newdp);
                }
            }
            catch(Exception ex)
            {


            }
        }

        public IDisposable Subscribe(IObserver<IDateTimeKeyPoint<TKey>> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(int count)
        {
            this.count = count;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(IDateTimeKeyPoint<TKey> item)
        {
            Task.Run(() => AddToDataPoints(KeyValuePair.Create(item.Key, KeyValuePair.Create(item.DateTime, item.Value))))
                .ToObservable()
                .Subscribe(refreshSubject.OnNext);
        }
    }
}

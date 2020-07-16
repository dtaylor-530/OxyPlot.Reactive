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
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System.Reactive.Concurrency;
using Kaos.Collections;

namespace OxyPlot.Reactive
{


    public class TimeModel<TKey> : TimeModel<TKey, ITimePoint<TKey>>
    {
        public TimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
            => collection
            .Scan(new TimePoint<TKey>(), (xy0, xy) => new TimePoint<TKey>(xy.Value.Key, Combine(xy0.Value, xy.Value.Value), xy.Key))
            .Cast<ITimePoint<TKey>>()
            .Skip(1);

    }


    public abstract class TimeModel<TKey, TType> : MultiPlotModel<TKey, DateTime>, IObservable<TType>, IObserver<int> where TType : ITimePoint<TKey>
    {
        readonly Subject<TType> subject = new Subject<TType>();
        protected readonly List<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> list = new List<KeyValuePair<TKey, KeyValuePair<DateTime, double>>>();
        protected int? count;
        protected DateTime min = DateTime.MaxValue, max = DateTime.MinValue;

        public TimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override void ModifyPlotModel()
        {
            plotModel.Axes.Add(new DateTimeAxis());
        }

        protected override async void Refresh(IList<Unit> units)
        {

            await Task.Run(() =>
            {
                lock (list)
                {
                    if (list.Any())
                    {
                        AddToDataPoints(list.ToArray());
                        list.Clear();
                    }
                }
            });

            _ = (this as IMixedScheduler).ScheduleAction(async () =>
              {
                  KeyValuePair<TKey, ICollection<KeyValuePair<DateTime, double>>>[]? dataPoints;
                  lock (DataPoints)
                      dataPoints = DataPoints.ToArray();

                  this.Modify();

                  foreach (var keyValue in dataPoints)
                  {
                      _ = await Task.Run(() =>
                      {
                            lock (DataPoints)
                            {
                               return Create(Flatten(keyValue.Key, keyValue.Value));
                            }
                        }).ContinueWith(async points => AddToSeries(await points, keyValue.Key.ToString()));
                  }

                  if (showAll)
                  {
                      _ = await Task.Run(() =>
                        {
                            lock (DataPoints)
                            {
                               return Create(Flatten(default, DataPoints.SelectMany(a => a.Value)));
                            }
                        }).ContinueWith(async points => AddToSeries(await points, "All"));
                  }
                  plotModel.InvalidatePlot(true);
              });

            IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> Flatten(TKey key, IEnumerable<KeyValuePair<DateTime, double>> value)
            => value.ToArray().Select(c => KeyValuePair.Create(key, c));
        }

        protected virtual TType[] Create(IEnumerable<KeyValuePair<TKey,KeyValuePair<DateTime, double>>> value)
        {
            return count.HasValue ?
                              Enumerable.TakeLast(ToDataPoints(value), count.Value).ToArray() :
                              ToDataPoints(value).ToArray();
        }

        protected virtual void Modify()
        {

        }
   

        protected virtual void AddToSeries(TType[] items, string title)
        {
            lock (plotModel)
            {
                if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
                {
                    series = OxyFactory.Build(items.Cast<DataPoint>(), title);

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
            lock (list)
                list.Add(item);
        }

        protected abstract IEnumerable<TType> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection);


        protected void AddToDataPoints(ICollection<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> items)
        {
            try
            {
                min = new DateTime(Math.Min(items.Min(a => a.Value.Key.Ticks), min.Ticks));
                max = new DateTime(Math.Max(items.Max(a => a.Value.Key.Ticks), max.Ticks));

                lock (DataPoints)
                {
                    foreach (var item in items)
                    {
                        if (!DataPoints.ContainsKey(item.Key))
                            DataPoints[item.Key] = new RankedMap<DateTime, double>();
                        DataPoints[item.Key].Add(item.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;

            }
        }


        public IDisposable Subscribe(IObserver<TType> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(int count)
        {
            this.count = count;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(ITimePoint<TKey> item)
        {
            AddToDataPoints(KeyValuePair.Create(item.Key, KeyValuePair.Create(item.DateTime, item.Value)));
            refreshSubject.OnNext(Unit.Default);
        }
    }
}

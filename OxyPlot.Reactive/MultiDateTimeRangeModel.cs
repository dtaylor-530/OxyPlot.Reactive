#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System.Reactive.Concurrency;
using Exceptionless.DateTimeExtensions;

namespace OxyPlot.Reactive
{
    public class MultiDateTimeRangeModel<TKey> : MultiDateTimeModel<TKey>, IObserver<TimeSpan>
    {
        private RangeType rangeType = RangeType.None;
        private DateTimeRange? dateTimeRange;
        private TimeSpan? timeSpan;

        public MultiDateTimeRangeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }


        protected override void Refresh(IList<Unit> units)
        {
            scheduler.Schedule(async () =>
            {
                KeyValuePair<TKey, ICollection<KeyValuePair<DateTime, double>>>[]? arr = default;
                lock (DataPoints)
                    arr = DataPoints.ToArray();

                foreach (var keyValue in arr)
                {
                    _ = await Task.Run(() =>
                      {
                          return Switch(keyValue.Value.ToArray().Select(c => KeyValuePair.Create(keyValue.Key, c))).ToArray();

                      }).ContinueWith(async points =>
                          AddToSeries(await points, keyValue.Key.ToString()));

                    if (showAll)
                    {
                        _ = await Task.Run(() =>
                        {
                            lock (DataPoints)
                            {
                                return Switch(arr.SelectMany(a => a.Value.Select(c => KeyValuePair.Create(a.Key, c)))).ToArray();
                            }

                        }).ContinueWith(async points =>
                            AddToSeries(await points, "All"));
                    }
                    lock (plotModel)
                        plotModel.InvalidatePlot(true);
                }
            });

            IEnumerable<IDateTimeKeyPoint<TKey>> Switch(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> col)
            {
                return rangeType switch
                {
                    RangeType.None => ToDataPoints(col),
                    RangeType.Count when count.HasValue => Enumerable.TakeLast(ToDataPoints(col), count.Value),
                    RangeType.TimeSpan when timeSpan.HasValue => ToDataPoints(col.ToArray().Filter(timeSpan.Value, a => a.Value.Key)),
                    RangeType.DateTimeRange when dateTimeRange != null => ToDataPoints(col.Filter(dateTimeRange, a => a.Value.Key)),
                    _ => throw new ArgumentOutOfRangeException("fdssffd")
                };
            }
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

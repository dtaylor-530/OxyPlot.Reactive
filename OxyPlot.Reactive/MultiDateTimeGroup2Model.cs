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
using MoreLinq;
using Exceptionless.DateTimeExtensions;
using System.Reactive.Subjects;

namespace OxyPlot.Reactive
{
    public class MultiDateTimeGroup2Model<TKey> : MultiDateTimeModel<TKey>, IObserver<TimeSpan>, IObservable<DateTimeRange[]>
    {
        private RangeType rangeType = RangeType.None;
        private TimeSpan? timeSpan;
        private DateTimeRange[]? ranges;
        private Subject<DateTimeRange[]> rangesSubject = new Subject<DateTimeRange[]>();
        public MultiDateTimeGroup2Model(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }



        protected override async void Refresh(IList<Unit> units)
        {
            if (!await Task.Run(() =>
            {
                lock (list)
                {
                    if (list.Any())
                    {
                        AddToDataPoints(list.ToArray());
                        list.Clear();
                        return true;
                    }
                    else
                        return true;
                }
            })) return;

            (this as IMixedScheduler).ScheduleAction(async () =>
            {
                KeyValuePair<TKey, ICollection<KeyValuePair<DateTime, double>>>[]? arr = default;
                lock (DataPoints)
                    arr = DataPoints.ToArray();

                if (timeSpan.HasValue)
                {
                    ranges = await Task.Run(() =>
                    {
                        return EnumerateDateTimeRanges(min, max, timeSpan.Value).ToArray();
                    });
                    rangesSubject.OnNext(ranges);
                }

                foreach (var keyValue in arr)
                {
                    _ = await Task.Run(() =>
                    {
                        return Switch(keyValue.Value.ToArray().Select(c => KeyValuePair.Create(keyValue.Key, c))).ToArray();

                    }).ContinueWith(async points =>
                        AddToSeries(await points, keyValue.Key.ToString()));
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
                        AddToSeries(await points, "All"));
                }
                lock (plotModel)
                    plotModel.InvalidatePlot(true);
            });

            IEnumerable<IDateTimeKeyPoint<TKey>> Switch(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> col)
            {
                return rangeType switch
                {
                    RangeType.None => ToDataPoints(col),
                    //RangeType.Count when count.HasValue => Enumerable.TakeLast(ToDataPoints(col), count.Value),
                    RangeType.TimeSpan when timeSpan.HasValue => ToDataPoints(col),
                    _ => throw new ArgumentOutOfRangeException("fdssffd")
                };
            }


            static IEnumerable<DateTimeRange> EnumerateDateTimeRanges(DateTime minDateTime, DateTime maxDateTime, TimeSpan timeSpan)
            {
                var dtRange = new DateTimeRange(minDateTime, minDateTime += timeSpan);
                while (dtRange.End < maxDateTime)
                {
                    yield return dtRange = new DateTimeRange(minDateTime, minDateTime += timeSpan);
                }

            }
        }

        protected override IEnumerable<IDateTimeKeyPoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            var se = ranges != null ? ees.GroupBy(ranges, a => a.Value.Key)
                .Where(a => a.Any())
                .Select(ac =>
                {
                    var ss = ac.Scan(default(DateTimePoint<TKey>), (a, b) => new DateTimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key)).Cast<IDateTimeKeyPoint<TKey>>()
                  .Skip(1).ToArray();
                    return new DateTimeRangePoint<TKey>(ac.Key, ss);
                }).Cast<IDateTimeKeyPoint<TKey>>().ToArray() :
                ees.Scan(default(DateTimePoint<TKey>), (a, b) => new DateTimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key)).Cast<IDateTimeKeyPoint<TKey>>().Skip(1).ToArray(); ;

            return se;
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            rangeType = RangeType.TimeSpan;
            refreshSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<DateTimeRange[]> observer)
        {
            return rangesSubject.Subscribe(observer);
        }

        enum RangeType
        {
            None,
            Count = 1,
            TimeSpan,
        }
    }
}

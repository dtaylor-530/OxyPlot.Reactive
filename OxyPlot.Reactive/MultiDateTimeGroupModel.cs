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

namespace OxyPlot.Reactive
{
    public class MultiDateTimeGroupModel<TKey> : MultiDateTimeModel<TKey>, IObserver<TimeSpan>
    {
        private RangeType rangeType = RangeType.None;
        private TimeSpan? timeSpan;

        public MultiDateTimeGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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
                    //RangeType.Count when count.HasValue => Enumerable.TakeLast(ToDataPoints(col), count.Value),
                    RangeType.TimeSpan when timeSpan.HasValue => ToDataPoints(col),
                    _ => throw new ArgumentOutOfRangeException("fdssffd")
                };
            }
        }

        protected override IEnumerable<IDateTimeKeyPoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);



            return timeSpan.HasValue ? ees.GroupBy(timeSpan.Value, a => a.Value.Key)
                .Select(ac =>
                {
                    var ss = ac.Scan(default(DateTimePoint<TKey>), (a, b) => new DateTimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key)).Cast<IDateTimeKeyPoint<TKey>>()
                    .Skip(1).ToArray();
                    return new DateTimeRangePoint<TKey>(ac.Key, ss);
                }).Cast<IDateTimeKeyPoint<TKey>>().ToArray() :
                ees.Scan(default(DateTimePoint<TKey>), (a, b) => new DateTimePoint<TKey>(b.Value.Key, Combine(a.Value , b.Value.Value), b.Key)).Cast<IDateTimeKeyPoint<TKey>>().Skip(1).ToArray();

        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            rangeType = RangeType.TimeSpan;
            refreshSubject.OnNext(Unit.Default);
        }

        enum RangeType
        {
            None,
            Count = 1,
            TimeSpan,
        }
    }
}

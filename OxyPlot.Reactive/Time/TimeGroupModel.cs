#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System.Reactive.Concurrency;
using MoreLinq;

namespace OxyPlot.Reactive
{
    public class TimeGroupModel<TKey> : TimeModel<TKey>, IObserver<TimeSpan>
    {
        //private RangeType rangeType = RangeType.None;
        private TimeSpan? timeSpan;
        private Operation? operation;

        public TimeGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        //protected override ITimePoint<TKey>[] Create(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> col)
        //{
        //    return rangeType switch
        //    {
        //        RangeType.None => ToDataPoints(col).ToArray(),
        //        //RangeType.Count when count.HasValue => Enumerable.TakeLast(ToDataPoints(col), count.Value),
        //        RangeType.TimeSpan when timeSpan.HasValue => ToDataPoints(col).ToArray(),
        //        _ => throw new ArgumentOutOfRangeException("fdssffd")
        //    };
        //}


        protected override IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            if (timeSpan.HasValue)
            {
                var arr = ees.GroupOn(timeSpan.Value, a => a.Value.Key).ToArray();
                return arr.Select(ac =>
                {
                    var ss = ac.Scan(default(TimePoint<TKey>), (a, b) => new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key)).Cast<ITimePoint<TKey>>()
                    .Skip(1).ToArray();
                    return new TimeRangePoint<TKey>(ac.Key, ss, ac.FirstOrDefault().Key, this.operation.HasValue ? operation.Value : Operation.Mean);
                }).Cast<ITimePoint<TKey>>().ToArray();
            }

            return ees.Scan(default(TimePoint<TKey>), (a, b) => new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key)).Cast<ITimePoint<TKey>>().Skip(1).ToArray();
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            // rangeType = RangeType.TimeSpan;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(Operation value)
        {
            operation = value;
            refreshSubject.OnNext(Unit.Default);
        }
    }
}

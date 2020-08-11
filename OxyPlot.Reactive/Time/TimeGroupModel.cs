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
using LinqStatistics;
using DynamicData;

namespace OxyPlot.Reactive
{
    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupModel<TKey> : TimeModel<TKey, ITimeRangePoint<TKey>>, IObserver<TimeSpan>
    {
        private TimeSpan? timeSpan;
        private Operation? operation;

        public TimeGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<ITimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ordered = collection
                .OrderBy(a => a.Value.Key);

            if (timeSpan.HasValue)
            {
                var arr = ordered.GroupOn(timeSpan.Value, a => a.Value.Key).ToArray();
                return arr.Select(ac =>
                {
                    var ss = ac.Scan(default(TimePoint<TKey>), (a, b) => new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key)).Cast<ITimePoint<TKey>>()
                    .Skip(1).ToArray();
                    return new TimeRangePoint<TKey>(ac.Key, ss, ac.FirstOrDefault().Key, this.operation.HasValue ? operation.Value : Operation.Mean);
                }).Cast<ITimeRangePoint<TKey>>().ToArray();
            }

            return ordered
                .Scan(default(TimeRangePoint<TKey>), (a, b) => new TimeRangePoint<TKey>(new Range<DateTime>(b.Value.Key, b.Value.Key), new ITimePoint<TKey>[] { new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value)) }, b.Key))
                .Cast<ITimeRangePoint<TKey>>().Skip(1).ToArray();
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(Operation value)
        {
            operation = value;
            refreshSubject.OnNext(Unit.Default);
        }
    }
}

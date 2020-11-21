#nullable enable

using LinqStatistics;
using MoreLinq;
using ReactivePlot.Base;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ReactivePlot.Time
{
    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupModel<TKey> : TimeGroupModel<TKey, TKey>
    {

        public TimeGroupModel(IPlotModel<ITimeRangePoint<TKey>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }
    }

    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupModel<TGroupKey, TKey> : TimeGroupBaseModel<TGroupKey, TKey, ITimeRangePoint<TKey>>
    {

        public TimeGroupModel(IPlotModel<ITimeRangePoint<TKey>> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }

        protected override ITimeRangePoint<TKey> CreatePoint(ITimeRangePoint<TKey>? timePoint0, IGrouping<Range<DateTime>, KeyValuePair<TGroupKey, ITimePoint<TKey>>> timePoints)
        {

            var points = ToDataPoints(timePoints.Select(a => a.Value), timePoint0?.Collection.Last()).ToArray();
            return new TimeRangePoint<TKey>(timePoints.Key, points, timePoints.FirstOrDefault().Value.Key, this.operation.HasValue ? operation.Value : Operation.Mean);

            IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<ITimePoint<TKey>> timePoints, ITimePoint<TKey>? timePoint0)
            {
                var ss = timePoints
                        .Scan(timePoint0, (a, b) => CreatePoint(a, b))
                        .Skip(1);

                return ss;
            }
        }
    }
}
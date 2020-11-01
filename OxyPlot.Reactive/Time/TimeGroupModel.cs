#nullable enable

using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LinqStatistics;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Base;

namespace OxyPlot.Reactive
{
    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupModel<TKey> : TimeGroupModel<TKey, TKey>
    {

        public TimeGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }
    }

    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupModel<TGroupKey, TKey> : TimeGroupBaseModel<TGroupKey, TKey, ITimeRangePoint<TKey>>
    {

        public TimeGroupModel(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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

        //protected override IEnumerable<ITimeRangePoint<TKey>> NoRanges(IOrderedEnumerable<KeyValuePair<TGroupKey, ITimePoint<TKey>>>? ees)
        //{ 
        //    return ees
        //            .Select(a => a.Value)
        //        .Select(a => (ITimeRangePoint<TKey>)new TimeRangePoint<TKey>(new Range<DateTime>(a.Var, a.Var), new ITimePoint<TKey>[] { a }, a.Key));
        //}
    
    }
}
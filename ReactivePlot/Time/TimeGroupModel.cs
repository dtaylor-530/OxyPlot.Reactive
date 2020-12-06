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

        public TimeGroupModel(IMultiPlotModel<ITimeRangePoint<TKey>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }
    }

    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupModel<TGroupKey, TKey> : TimeGroupBaseModel<TGroupKey, TKey, ITimeRangePoint<TKey>>
    {

        public TimeGroupModel(IMultiPlotModel<ITimeRangePoint<TKey>> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }

        protected override ITimeRangePoint<TKey> CreatePoint(ITimeRangePoint<TKey>? timePoint0, IGrouping<Range<DateTime>,ITimePoint<TKey>> timePoints)
        {
            var points = ToDataPoints(timePoints, timePoint0?.Collection.Last()).ToArray();
            return new TimeRangePoint<TKey>(timePoints.Key, points, timePoints.FirstOrDefault().Key, this.operation.HasValue ? operation.Value : Operation.Mean);

        }

        protected virtual IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<ITimePoint<TKey>> timePoints, ITimePoint<TKey>? timePoint0)
        {
            var ses = timePoints
                    .Scan(timePoint0, (a, b) => CreatePoint(a, b))
                    .Skip(1);

            return ses;
        }

        protected override ITimeRangePoint<TKey> CreateNewPoint(ITimeRangePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            throw new NotImplementedException();
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            throw new NotImplementedException();
        }

        protected override TGroupKey GetKey(ITimePoint<TKey> item)
        {
            throw new NotImplementedException();
        }
    }
}
#nullable enable

using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LinqStatistics;
using OnTheFlyStats;
using OxyPlot.Reactive.Model.Enum;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Base;
using ReactivePlot.Model;

namespace OxyPlot.Reactive
{




    public interface ITimeStatsRangePoint<TKey> : ITimeRangePoint<TKey, ITimeStatsPoint<TKey>>, ITimeStatsPoint<TKey>
    {
    }

    public class TimeStatsRangePoint<TKey> : TimeRangePoint<TKey, ITimeStatsPoint<TKey>>, ITimeStatsRangePoint<TKey>
    {


        public TimeStatsRangePoint(Range<DateTime> dateTimeRange, ICollection<ITimeStatsPoint<TKey>> value, Stats value2, TKey key) : this(dateTimeRange, value, value2, key, Operation.Mean)
        {
        }

        public TimeStatsRangePoint(Range<DateTime> dateTimeRange, ICollection<ITimeStatsPoint<TKey>> value, Stats value2) : this(dateTimeRange, value, value2, default, Operation.Mean)
        {
        }

        public TimeStatsRangePoint(Range<DateTime> timeRange, ICollection<ITimeStatsPoint<TKey>> value, Stats model, TKey key, Operation operation) : base(timeRange, value, key, operation)
        {
            Model = model;
        }

        public Stats Model { get; }
    }


    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupOnTheFlyStatsModel<TKey> : TimeGroupOnTheFlyStatsModel<TKey, TKey>
    {

        public TimeGroupOnTheFlyStatsModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }
    }


    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupOnTheFlyStatsModel<TGroupKey, TKey> : TimeGroupBaseModel<TGroupKey, TKey, ITimeStatsPoint<TKey>, ITimeStatsRangePoint<TKey>>, IObserver<RollingOperation>
    {
        private RollingOperation? rollingOperation;


        public TimeGroupOnTheFlyStatsModel(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }


        public void OnNext(RollingOperation value)
        {
            this.rollingOperation = value;
            refreshSubject.OnNext(Unit.Default);
        }


        protected override ITimeStatsRangePoint<TKey> CreatePoint(ITimeStatsRangePoint<TKey>? timePoint0, IGrouping<Range<DateTime>, KeyValuePair<TGroupKey, ITimeStatsPoint<TKey>>> timePoints)
        {

            var points = ToDataPoints(timePoints.Select(a => a.Value), timePoint0?.Collection.Last()).ToArray();
            return new TimeStatsRangePoint<TKey>(timePoints.Key, points, timePoint0?.Model ?? new OnTheFlyStats.Stats(), timePoints.FirstOrDefault().Value.Key, this.operation.HasValue ? operation.Value : Operation.Mean);

            IEnumerable<ITimeStatsPoint<TKey>> ToDataPoints(IEnumerable<ITimeStatsPoint<TKey>> timePoints, ITimeStatsPoint<TKey>? timePoint0)
            {
                var ss = timePoints
                        .Scan(timePoint0, (a, b) => CreatePoint(a, b))
                        .Skip(1);

                return ss;

            }
        }

        protected override ITimeStatsPoint<TKey> CreatePoint(ITimeStatsPoint<TKey> xy0, ITimeStatsPoint<TKey> xy)
        {
            return OnTheFlyStatsHelper.Combine(xy0, xy, rollingOperation);
        }
    }
}
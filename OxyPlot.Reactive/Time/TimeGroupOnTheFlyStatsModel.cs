#nullable enable

using MoreLinq;
using OxyPlot.Reactive.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    using DynamicData;
    using LinqStatistics;
    using Model;
    using OnTheFlyStats;
    using OxyPlot.Axes;
    using OxyPlot.Reactive.Model.Enum;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public interface ITimeStatsRangePoint<TKey> : ITimeRangePoint<TKey, ITimeStatsPoint<TKey>>, ITimeStatsPoint<TKey>
    {
    }

    public class TimeStatsRangePoint<TKey> : TimeRangePoint<TKey, ITimeStatsPoint<TKey>>, ITimeStatsRangePoint<TKey>
    {


        public TimeStatsRangePoint(Range<DateTime> dateTimeRange, ICollection<ITimeStatsPoint<TKey>> value, OnTheFlyStats.Stats value2, TKey key) : this(dateTimeRange, value, value2, key, Operation.Mean)
        {
        }

        public TimeStatsRangePoint(Range<DateTime> dateTimeRange, ICollection<ITimeStatsPoint<TKey>> value, OnTheFlyStats.Stats value2) : this(dateTimeRange, value, value2, default, Operation.Mean)
        {
        }

        public TimeStatsRangePoint(Range<DateTime> timeRange, ICollection<ITimeStatsPoint<TKey>> value, OnTheFlyStats.Stats value2, TKey key, Operation operation) : base(timeRange, value, key, operation)
        {
            Value2 = value2;
        }

        public Stats Value2 { get; }
    }

    //public class TimeStatsRangePoint<TKey> : TimeRangePoint<TKey>, ITimeStatsRangePoint<TKey>
    //{
    //    public TimeStatsRangePoint(Range<DateTime> dateTimeRange, ICollection<IPoint<DateTime, double>> value,  TKey key) : this(dateTimeRange, value,  key, Operation.Mean)
    //    {
    //    }

    //    public TimeStatsRangePoint(Range<DateTime> dateTimeRange, ICollection<IPoint<DateTime, double>> value) : this(dateTimeRange, value, default, Operation.Mean)
    //    {
    //    }

    //    public TimeStatsRangePoint(Range<DateTime> timeRange, ICollection<IPoint<DateTime, double>> value, TKey key, Operation operation) : base(timeRange, value, key, operation)
    //    {
    //    }
    //}

    public interface ITimeStatsPoint<TKey> : ITime2Point<TKey, OnTheFlyStats.Stats>
    {

    }


    public class TimeStatsPoint<TKey> : ITimeStatsPoint<TKey>
    {
        public TimeStatsPoint(DateTime dateTime, double value, OnTheFlyStats.Stats value2, TKey key)
        {
            Var = dateTime;
            Value = value;
            Value2 = value2;
            this.Key = key;
        }

        public TimeStatsPoint(DateTime dateTime, double value, OnTheFlyStats.Stats value2) : this(dateTime, value, value2, default)
        {
        }



        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public OnTheFlyStats.Stats Value2 { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }

        public override string ToString() => $"{Var:F}, {Value}, {Key}";

        public static ITimeStatsPoint<TKey> Create(DateTime dateTime, double value, OnTheFlyStats.Stats value2, TKey key)
        {
            return new TimeStatsPoint<TKey>(dateTime, value, value2, key);
        }
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
    public class TimeGroupOnTheFlyStatsModel<TGroupKey, TKey> : TimeGroup2Model<TGroupKey, TKey, ITimeStatsPoint<TKey>, ITimeStatsRangePoint<TKey>>, IObserver<RollingOperation>
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
            return new TimeStatsRangePoint<TKey>(timePoints.Key, points, timePoint0.Value2, timePoints.FirstOrDefault().Value.Key, this.operation.HasValue ? operation.Value : Operation.Mean);

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
            throw new NotImplementedException();
        }

        protected override IEnumerable<ITimeStatsRangePoint<TKey>> NoRanges(IOrderedEnumerable<KeyValuePair<TGroupKey, ITimeStatsPoint<TKey>>>? ees)
        {
            return ees
                    .Select(a => a.Value)
                .Select(a => (ITimeStatsRangePoint<TKey>)new TimeStatsRangePoint<TKey>(new Range<DateTime>(a.Var, a.Var), new ITimeStatsPoint<TKey>[] { a }, a.Value2, a.Key));
        }
    }
}
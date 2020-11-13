using LinqStatistics;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Model
{

    public class TimeRangePoint<TKey,TType> : RangePoint<TKey, DateTime, TType>, ITimeRangePoint<TKey, TType>
            where TType : ITimePoint<TKey>
    {
        public TimeRangePoint(Range<DateTime> dateTimeRange, ICollection<TType> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }

        public TimeRangePoint(Range<DateTime> dateTimeRange, ICollection<TType> value) : this(dateTimeRange, value, default, Operation.Mean)
        {
        }

        public TimeRangePoint(Range<DateTime> timeRange, ICollection<TType> value, TKey key, Operation operation) : base(timeRange, value, key, operation)
        {
        }

        public override DateTime Var => new DateTime((long)(Range.Min.Ticks + (Range.Max - Range.Min).Ticks / 2d));

        public override double Value => Collection.Count > 1 ? GetValue() : Collection.Single().Value;

        public override DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }
    }

    public class TimeRangePoint<TKey> : RangePoint<TKey, DateTime, ITimePoint<TKey>>, ITimeRangePoint<TKey>
    {
        public TimeRangePoint(Range<DateTime> dateTimeRange, ICollection<ITimePoint<TKey>> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }

        public TimeRangePoint(Range<DateTime> dateTimeRange, ICollection<ITimePoint<TKey>> value) : this(dateTimeRange, value, default, Operation.Mean)
        {
        }

        public TimeRangePoint(Range<DateTime> timeRange, ICollection<ITimePoint<TKey>> value, TKey key, Operation operation) : base(timeRange, value, key, operation)
        {
        }

        public override DateTime Var => new DateTime((long)(Range.Min.Ticks + (Range.Max - Range.Min).Ticks / 2d));

        public override double Value => Collection.Count > 1 ? GetValue() : Collection.Single().Value;

        public override DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }
    }


    public class Time2RangePoint<TKey, TModel> : RangePoint<TKey, DateTime, ITimePoint<TKey>>, ITime2RangePoint<TKey, TModel>
    {


        public Time2RangePoint(Range<DateTime> dateTimeRange, ICollection<ITimePoint<TKey>> value, TModel value2, TKey key) : this(dateTimeRange, value, value2, key, Operation.Mean)
        {
        }

        public Time2RangePoint(Range<DateTime> dateTimeRange, ICollection<ITimePoint<TKey>> value, TModel value2) : this(dateTimeRange, value, value2, default, Operation.Mean)
        {
        }

        public Time2RangePoint(Range<DateTime> timeRange, ICollection<ITimePoint<TKey>> value, TModel value2, TKey key, Operation operation) : base(timeRange, value, key, operation)
        {
            Model = value2;
        }

        public override DateTime Var => new DateTime((long)(Range.Min.Ticks + (Range.Max - Range.Min).Ticks / 2d));

        public TModel Model { get; }

        public override DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }
    }
}
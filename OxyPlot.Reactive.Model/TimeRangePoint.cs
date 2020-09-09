using LinqStatistics;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;

namespace OxyPlot.Reactive.Model
{
    public class TimeRangePoint<TKey> : RangePoint<TKey, DateTime>, ITimeRangePoint<TKey>
    {
        public TimeRangePoint(Range<DateTime> timeRange, ICollection<IPoint<DateTime, double>> value, TKey key, Operation operation) : base(timeRange, value, key, operation)
        {
        }

        public TimeRangePoint(Range<DateTime> dateTimeRange, ICollection<IPoint<DateTime, double>> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }

        public TimeRangePoint(Range<DateTime> dateTimeRange, ICollection<IPoint<DateTime, double>> value) : this(dateTimeRange, value, default, Operation.Mean)
        {
        }

        public override DateTime Var => new DateTime((long)(Range.Min.Ticks + (Range.Max - Range.Min).Ticks / 2d));

        public override DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }
    }
}
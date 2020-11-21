using LinqStatistics;
using ReactivePlot.Model.Enum;
using System.Collections.Generic;

namespace ReactivePlot.Model
{
    public class DoubleRangePoint<TKey> : RangePoint<TKey, double, IDoublePoint<TKey>>, IDoubleRangePoint<TKey>
    {
        public DoubleRangePoint(Range<double> dateTimeRange, ICollection<IDoublePoint<TKey>> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }

        public DoubleRangePoint(Range<double> dateTimeRange, ICollection<IDoublePoint<TKey>> value) : this(dateTimeRange, value, default, Operation.Mean)
        {
        }

        public DoubleRangePoint(Range<double> timeRange, ICollection<IDoublePoint<TKey>> value, TKey key, Operation operation) : base(timeRange, value, key, operation)
        {
        }

        public override double Var => Range.Min + (Range.Max - Range.Min) / 2d;



        public override (double, double) DataPoint()
        {
            return (Var, Value);
        }
    }
}
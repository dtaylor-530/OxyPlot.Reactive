using LinqStatistics;
using System.Collections.Generic;

namespace OxyPlot.Reactive.Model
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



        public override DataPoint GetDataPoint()
        {
            return new DataPoint(Var, Value);
        }
    }
}
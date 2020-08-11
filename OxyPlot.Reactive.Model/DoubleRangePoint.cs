using LinqStatistics;
using System.Collections.Generic;

namespace OxyPlot.Reactive.Model
{
    public class DoubleRangePoint<TKey> : RangePoint<TKey, double>, IDoubleRangePoint<TKey>
    {
        public DoubleRangePoint(Range<double> timeRange, ICollection<IPoint<double, double>> value, TKey key, Operation operation) : base(timeRange, value, key, operation)
        {
        }

        public DoubleRangePoint(Range<double> dateTimeRange, ICollection<IPoint<double, double>> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }
        public DoubleRangePoint(Range<double> dateTimeRange, ICollection<IPoint<double, double>> value) : this(dateTimeRange, value, default, Operation.Mean)
        {
        }

        public override double Var => Range.Min + (Range.Max - Range.Min) / 2d;

        public override DataPoint GetDataPoint()
        {
            return new DataPoint(Var, Value);
        }
    }
}

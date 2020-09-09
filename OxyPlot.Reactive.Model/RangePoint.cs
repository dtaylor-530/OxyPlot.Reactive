using Endless;
using LinqStatistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Model
{
    public abstract class RangePoint<TKey, TVar, TValue> : IRangePoint<TKey, TVar, TValue> where TVar : struct, IComparable<TVar>, IFormattable, IEquatable<TVar>
    {
        public RangePoint(Range<TVar> timeRange, ICollection<IPoint<TVar, TValue>> value, TKey key)
        {
            Range = timeRange;
            Collection = value;
            this.Key = key;
        }

        public RangePoint(Range<TVar> dateTimeRange, ICollection<IPoint<TVar, TValue>> value) : this(dateTimeRange, value, default)
        {
        }

        public TKey Key { get; }

        public Range<TVar> Range { get; }

        public abstract TVar Var { get; }

        public abstract TValue Value { get; }

        public ICollection<IPoint<TVar, TValue>> Collection { get; }

        public abstract DataPoint GetDataPoint();

        public override string ToString()
        {
            return $"{Var:F}, {Value}";
        }

        //public static ITimePoint<TKey> Create(ITimeRange dateTimeRange, ICollection<ITimePoint<TKey>> value, TKey key)
        //{
        //    return new TRangePoint<TKey>(dateTimeRange, value, key);
        //}
    }

    public abstract class RangePoint<TKey, TVar> : RangePoint<TKey, TVar, double> where TVar : struct, IComparable<TVar>, IFormattable, IEquatable<TVar>
    {
        private readonly Operation operation;

        public RangePoint(Range<TVar> timeRange, ICollection<IPoint<TVar, double>> value, TKey key, Operation operation) : base(timeRange, value, key)
        {
            this.operation = operation;
        }

        public RangePoint(Range<TVar> dateTimeRange, ICollection<IPoint<TVar, double>> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }

        public RangePoint(Range<TVar> dateTimeRange, ICollection<IPoint<TVar, double>> value) : this(dateTimeRange, value, default, Operation.Mean)
        {
        }

        public override double Value => Collection.Count > 1 ? GetValue() : Collection.Single().Value;

        private double GetValue()
        {
            switch (this.operation)
            {
                case Operation.Mean: return Collection.Average(a => a.Value);
                case Operation.Variance: return Collection.Variance(a => a.Value);
                case Operation.StandardDeviation: return Collection.StandardDeviation(a => a.Value);
                case Operation.Mode: return Collection.Mode(a => a.Value) ?? 0d;
                case Operation.Max: return Collection.Max(a => a.Value);
                case Operation.Min: return Collection.Min(a => a.Value);
                case Operation.Median: return Collection.Median(a => a.Value);
                case Operation.Random: return Collection.Random().Value;
                case Operation.First: return Collection.First().Value;
                case Operation.Last: return Collection.Last().Value;
                case Operation.Difference: return Collection.Last().Value - Collection.First().Value;
                case Operation.Sum: return Collection.Sum(a => a.Value);
                case Operation.Middle: return Collection.ElementAt((int)(Collection.Count / 2d)).Value;
                default: throw new NotImplementedException();
            };
        }

        public override string ToString()
        {
            return $"{Var:F}, {Value}";
        }
    }
}
using Endless;
using LinqStatistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Model
{

    public abstract class RangePoint<TKey, TVar> : RangePoint<TKey, TVar, IPoint<TVar, double>>
    where TVar : struct, IComparable<TVar>, IFormattable, IEquatable<TVar>
    {
        public RangePoint(Range<TVar> dateTimeRange, ICollection<IPoint<TVar, double>> collection, TKey key) : this(dateTimeRange, collection, key, Operation.Mean)
        {
        }

        public RangePoint(Range<TVar> dateTimeRange, ICollection<IPoint<TVar, double>> collection) : this(dateTimeRange, collection, default, Operation.Mean)
        {
        }

        public RangePoint(Range<TVar> timeRange, ICollection<IPoint<TVar, double>> collection, TKey key, Operation operation) : base(timeRange, collection, key, operation)
        {
        }
    }


    public abstract class RangePoint<TKey, TVar, TPoint> : RangePoint<TKey, TVar, double, TPoint>
        where TVar : struct, IComparable<TVar>, IFormattable, IEquatable<TVar>
        where TPoint : IPoint<TVar, double>
    {
        private readonly Operation operation;

        public RangePoint(Range<TVar> dateTimeRange, ICollection<TPoint> collection, TKey key) : this(dateTimeRange, collection, key, Operation.Mean)
        {
        }

        public RangePoint(Range<TVar> dateTimeRange, ICollection<TPoint> collection) : this(dateTimeRange, collection, default, Operation.Mean)
        {
        }

        public RangePoint(Range<TVar> timeRange, ICollection<TPoint> collection, TKey key, Operation operation) : base(timeRange, collection, key)
        {
            this.operation = operation;
        }


        public override double Value => Collection.Count > 1 ? GetValue() : Collection.Single().Value;

        protected virtual double GetValue()
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
    }

    public abstract class RangeBasePoint<TKey, TVar, TValue> : RangePoint<TKey, TVar, TValue, IPoint<TVar, TValue>> where TVar : struct, IComparable<TVar>, IFormattable, IEquatable<TVar>
    {
        public RangeBasePoint(Range<TVar> timeRange, ICollection<IPoint<TVar, TValue>> value, TKey key) : base(timeRange, value, key)
        {

        }

        public RangeBasePoint(Range<TVar> dateTimeRange, ICollection<IPoint<TVar, TValue>> value) : this(dateTimeRange, value, default)
        {
        }
    }


    public abstract class RangePoint<TKey, TVar, TValue, TPoint> : IRangePoint<TKey, TVar, TValue, TPoint>
    where TVar : struct, IComparable<TVar>, IFormattable, IEquatable<TVar>
    where TPoint : IPoint<TVar, TValue>
    {

        public RangePoint(Range<TVar> dateTimeRange, ICollection<TPoint> value) : this(dateTimeRange, value, default)
        {
        }

        public RangePoint(Range<TVar> timeRange, ICollection<TPoint> value, TKey key)
        {
            Range = timeRange;
            Collection = value;
            this.Key = key;
        }


        public TKey Key { get; }

        public Range<TVar> Range { get; }

        public abstract TVar Var { get; }

        public abstract TValue Value { get; }

        public ICollection<TPoint> Collection { get; }

        public abstract DataPoint GetDataPoint();

        public override string ToString()
        {
            return $"{Var:F}, {Value}";
        }
    }
}
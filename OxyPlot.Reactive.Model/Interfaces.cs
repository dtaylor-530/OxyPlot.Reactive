using LinqStatistics;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;

namespace OxyPlot.Reactive.Model
{
    public interface IValue<T>
    {
        T Value { get; }
    }

    public interface IValue2<T>
    {
        T Value2 { get; }
    }


    public interface IVar<T>
    {
        T Var { get; }
    }

    public interface IKey<TKey>
    {
        TKey Key { get; }
    }

    public interface IGroupKey<TKey>
    {
        TKey GroupKey { get; }
    }

    public interface IPoint<TVar, TValue> : IVar<TVar>, IValue<TValue>, IDataPointProvider
    {
    }

    public interface IKeyPoint<TKey, TVar, TValue> : IKey<TKey>, IPoint<TVar, TValue>
    {
    }

    public interface IDoublePoint<TKey, TVar> : IKeyPoint<TKey, TVar, double> where TVar : IComparable<TVar>
    {
    }

    public interface ITimePoint : IValue<double>, IVar<DateTime>, IDataPointProvider
    {
    }

    public interface IDoublePoint<TKey> : IDoublePoint<TKey, double>, IKey<TKey>
    {
    }

    public interface I2Point<TKey, TVar> : IDoublePoint<TKey, TVar>, IKey<TKey> where TVar : IComparable<TVar>
    {
    }

    public interface IRange<T> where T : struct, IComparable<T>, IFormattable, IEquatable<T>
    {
        Range<T> Range { get; }
    }

    public interface ITimeRange<T> : IRange<DateTime>
    {
    }

    public interface ITimePoint<TKey> : IDoublePoint<TKey, DateTime>, ITimePoint, IKey<TKey>, IPoint<DateTime, double>
    {
    }

    public interface ITime2Point<TKey> : ITimePoint<TKey>, IValue2<double>
    {
    }

    public interface ITime2Point<TKey, TValue> : ITimePoint<TKey>, IValue2<TValue>
    {
    }

    public interface ITimeGroupPoint<TGroupKey, TKey> : ITimePoint<TKey>, IGroupKey<TGroupKey>
    {
    }

    public interface ITimeGroupPoint<TKey> : ITimePoint<TKey>, IGroupKey<TKey>
    {
    }

    public interface ICollection
    {
    }

    public interface ITimePointCollection<TKey>
    {
        ICollection<ITimePoint<TKey>> Collection { get; }
    }

    public interface IDoublePointCollection<TKey>
    {
        ICollection<IDoublePoint<TKey>> Collection { get; }
    }

    public interface IPointCollection<TVar, TValue, TPoint> : IPoint<TVar, TValue>
    {
        ICollection<TPoint> Collection { get; }
    }

    public interface IPointCollection<TVar, TValue> : IPointCollection<TVar, TValue, IPoint<TVar, TValue>>
    {
    }

    public interface ITimeRangePoint<TKey> : ITimeRangePoint<TKey, ITimePoint<TKey>>
    {
    }

    //public interface ITimeRangePoint<TKey> : ITimePoint<TKey>, IPointCollection<DateTime, double, ITimePoint<TKey>>, ITimeRange<DateTime>
    //{
    //}

    public interface ITimeRangePoint<TKey, TType> : ITimePoint<TKey>, IPointCollection<DateTime, double, TType>, ITimeRange<DateTime>
        where TType : ITimePoint<TKey>
    {
    }

    public interface ITime2RangePoint<TKey, TValue2> : ITimeRangePoint<TKey>, ITime2Point<TKey, TValue2>
    {
    }

    public interface ITimeGroupRangePoint<TGroupKey, TKey> : ITimeGroupPoint<TGroupKey, TKey>, IPointCollection<DateTime, double, IDoublePoint<TKey>>, ITimeRange<DateTime>
    {
    }

    public interface IDoubleRangePoint<TKey> : IDoublePoint<TKey>, IPointCollection<double, double, IDoublePoint<TKey>>, IRange<double>
    {
    }

    public interface IRangePoint<TKey, TVar, TValue, TPoint> : IKeyPoint<TKey, TVar, TValue>, IPointCollection<TVar, TValue, TPoint>, IRange<TVar> where TVar : struct, IComparable<TVar>, IFormattable, IEquatable<TVar>
    {
    }

    public interface IDataPointKeyProvider<T> : IDataPointProvider, IKey<T>
    {
    }

    public interface IDateTimeKeyPointObserver<TType, TKey> : IObserver<TType> where TType : ITimePoint<TKey>
    {
    }
}
using LinqStatistics;
using System;
using System.Collections.Generic;

namespace OxyPlot.Reactive.Model
{
    public interface IValue<T>
    {
        T Value { get; }

    }

    public interface IVar<T>
    {
        T Var { get; }

    }

    public interface IKey<TKey>
    {
        TKey Key { get; }
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


    public interface IPointCollection<TVar, TValue>
    {
        ICollection<IPoint<TVar, TValue>> Collection { get; }

    }

    public interface ITimeRangePoint<TKey> : ITimePoint<TKey>, IPointCollection<DateTime, double>, ITimeRange<DateTime>
    {
    }

    public interface IDoubleRangePoint<TKey> : IDoublePoint<TKey>, IPointCollection<double, double>, IRange<double>
    {
    }

    public interface IRangePoint<TKey, TVar, TValue> : IKeyPoint<TKey, TVar, TValue>, IPointCollection<TVar, TValue>, IRange<TVar> where TVar : struct, IComparable<TVar>, IFormattable, IEquatable<TVar>
    {
    }

    public interface IDataPointKeyProvider<T> : IDataPointProvider, IKey<T>
    {
    }

    public interface IDateTimeKeyPointObserver<TType, TKey> : IObserver<TType> where TType : ITimePoint<TKey>
    {
    }
}

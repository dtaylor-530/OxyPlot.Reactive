using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Text;

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

    public interface IDoublePoint<TKey, TVar> : IKeyPoint<TKey, TVar, double>
    {
    }


    public interface ITimePoint : IValue<double>, IVar<DateTime>, IDataPointProvider
    {
    }

    public interface IPoint<TKey> : IDoublePoint<TKey, double>, IKey<TKey>
    {
    }


    public interface ITimePoint<TKey> : IDoublePoint<TKey, DateTime>, ITimePoint, IKey<TKey>
    {
    }

    public interface ICollection
    {

    }

    public interface IDateTimeKeyPointCollection<TKey>
    {
        ICollection<ITimePoint<TKey>> Collection { get; }
    }

    public interface IDateTimeRange
    {
        ITimeRange TimeRange { get; }
    }

    public interface ITimeRangePoint<TKey> : ITimePoint<TKey>, IDateTimeKeyPointCollection<TKey>, IDateTimeRange
    {

    }

    public interface IDataPointKeyProvider<T> : IDataPointProvider, IKey<T>
    {

    }

    public interface IDateTimeKeyPointObserver<TType, TKey> : IObserver<TType> where TType : ITimePoint<TKey>
    {
    }


}

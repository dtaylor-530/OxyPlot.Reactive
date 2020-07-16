using Exceptionless.DateTimeExtensions;
using LinqStatistics;
using NodaTime;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Model
{
    public interface IDateTimePoint : IDataPointProvider
    {
        DateTime DateTime { get; }

        double Value { get; }
    }

    public interface IKey<TKey>
    {
        TKey Key { get; }
    }

    public interface IDateTimePoint<TKey> : IDateTimePoint, IKey<TKey>
    {
    }

    public interface ICollection
    {

    }

    public interface IDateTimeKeyPointCollection<TKey>
    {
        public ICollection<IDateTimePoint<TKey>> Collection { get; }
    }

    public interface IDateTimeRange
    {
        public DateTimeRange DateTimeRange { get; }
    }

    public interface IDateTimeRangePoint<TKey> : IDateTimePoint<TKey>, IDateTimeKeyPointCollection<TKey>, IDateTimeRange
    {


    }

    public interface IDataPointKeyProvider<T> : IDataPointProvider, IKey<T>
    {

    }

    public interface IDateTimeKeyPointObserver<TType, TKey> : IObserver<TType> where TType: IDateTimePoint<TKey>
    {
    }

    public struct DateTimePoint : IDateTimePoint<string>
    {

        public DateTimePoint(DateTime dateTime, double value, string? key)
        {
            DateTime = dateTime;
            Value = value;
            Key = key ?? string.Empty;

        }

        public DateTimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
        {
        }

        public string Key { get; }

        public DateTime DateTime { get; }

        public double Value { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(DateTime), Value);
        }

        public override string ToString()
        {
            return $"{DateTime:F}, {Value}, {Key}";
        }

        public static IDateTimePoint<string> Create(DateTime dateTime, double value, string key)
        {
            return new DateTimePoint(dateTime, value, key);
        }
    }

    public struct DateTimePoint<TKey> : IDateTimePoint<TKey>
    {

        public DateTimePoint(DateTime dateTime, double value, TKey key)
        {
            DateTime = dateTime;
            Value = value;
            this.Key = key;

        }

        public DateTimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
        {
        }

        public TKey Key { get; }

        public DateTime DateTime { get; }

        public double Value { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(DateTime), Value);
        }

        public override string ToString()
        {
            return $"{DateTime:F}, {Value}, {Key}";
        }

        public static IDateTimePoint<TKey> Create(DateTime dateTime, double value, TKey key)
        {
            return new DateTimePoint<TKey>(dateTime, value, key);
        }
    }

    public class DateTimeRangePoint<TKey> : IDateTimeRangePoint<TKey>
    {
        private readonly Operation operation;

        public DateTimeRangePoint(DateTimeRange dateTimeRange, ICollection<IDateTimePoint<TKey>> value, TKey key, Operation operation)
        {
            DateTimeRange = dateTimeRange;
            Collection = value;
            this.Key = key;
            this.operation = operation;
        }

        public DateTimeRangePoint(DateTimeRange dateTimeRange, ICollection<IDateTimePoint<TKey>> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }
        public DateTimeRangePoint(DateTimeRange dateTimeRange, ICollection<IDateTimePoint<TKey>> value) : this(dateTimeRange, value, default, Operation.Mean)
        {
        }
        public TKey Key { get; }

        public DateTimeRange DateTimeRange { get; }


        public virtual DateTime DateTime => DateTimeRange.Start + (DateTimeRange.End - DateTimeRange.Start) / 2;

        //     public virtual double Value => Collection.Count>1? Collection.Average(a => a.Value): Collection.First().Value;

        public virtual double Value

           => Collection.Count > 1 ? this.operation switch
           {
               Operation.Mean => Collection.Average(a => a.Value),
               Operation.Variance => Collection.Variance(a => a.Value),
               Operation.StandardDeviation => Collection.StandardDeviation(a => a.Value),
               Operation.Mode => Collection.Mode(a => a.Value) ?? 0,
               Operation.Max => Collection.Max(a => a.Value),
               Operation.Min => Collection.Min(a => a.Value),
               Operation.Median => Collection.Median(a => a.Value),
               _ => throw new NotImplementedException()
           } : Collection.Single().Value;




        public ICollection<IDateTimePoint<TKey>> Collection { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(DateTime), Value);
        }

        public override string ToString()
        {
            return $"{DateTime:F}, {Value}";
        }

        public static IDateTimePoint<TKey> Create(DateTimeRange dateTimeRange, ICollection<IDateTimePoint<TKey>> value, TKey key)
        {
            return new DateTimeRangePoint<TKey>(dateTimeRange, value, key);
        }
    }
}

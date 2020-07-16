using Exceptionless.DateTimeExtensions;
using LinqStatistics;
using NodaTime;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Model
{
    public interface ITimePoint : IDataPointProvider
    {
        DateTime DateTime { get; }

        double Value { get; }
    }

    public interface IKey<TKey>
    {
        TKey Key { get; }
    }

    public interface ITimePoint<TKey> : ITimePoint, IKey<TKey>
    {
    }

    public interface ICollection
    {

    }

    public interface IDateTimeKeyPointCollection<TKey>
    {
        public ICollection<ITimePoint<TKey>> Collection { get; }
    }

    public interface IDateTimeRange
    {
        public DateTimeRange DateTimeRange { get; }
    }

    public interface ITimeRangePoint<TKey> : ITimePoint<TKey>, IDateTimeKeyPointCollection<TKey>, IDateTimeRange
    {


    }

    public interface IDataPointKeyProvider<T> : IDataPointProvider, IKey<T>
    {

    }

    public interface IDateTimeKeyPointObserver<TType, TKey> : IObserver<TType> where TType: ITimePoint<TKey>
    {
    }

    public struct TimePoint : ITimePoint<string>
    {

        public TimePoint(DateTime dateTime, double value, string? key)
        {
            DateTime = dateTime;
            Value = value;
            Key = key ?? string.Empty;

        }

        public TimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
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

        public static ITimePoint<string> Create(DateTime dateTime, double value, string key)
        {
            return new TimePoint(dateTime, value, key);
        }
    }

    public struct TimePoint<TKey> : ITimePoint<TKey>
    {

        public TimePoint(DateTime dateTime, double value, TKey key)
        {
            DateTime = dateTime;
            Value = value;
            this.Key = key;

        }

        public TimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
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

        public static ITimePoint<TKey> Create(DateTime dateTime, double value, TKey key)
        {
            return new TimePoint<TKey>(dateTime, value, key);
        }
    }

    public class TimeRangePoint<TKey> : ITimeRangePoint<TKey>
    {
        private readonly Operation operation;

        public TimeRangePoint(DateTimeRange dateTimeRange, ICollection<ITimePoint<TKey>> value, TKey key, Operation operation)
        {
            DateTimeRange = dateTimeRange;
            Collection = value;
            this.Key = key;
            this.operation = operation;
        }

        public TimeRangePoint(DateTimeRange dateTimeRange, ICollection<ITimePoint<TKey>> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }
        public TimeRangePoint(DateTimeRange dateTimeRange, ICollection<ITimePoint<TKey>> value) : this(dateTimeRange, value, default, Operation.Mean)
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




        public ICollection<ITimePoint<TKey>> Collection { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(DateTime), Value);
        }

        public override string ToString()
        {
            return $"{DateTime:F}, {Value}";
        }

        public static ITimePoint<TKey> Create(DateTimeRange dateTimeRange, ICollection<ITimePoint<TKey>> value, TKey key)
        {
            return new TimeRangePoint<TKey>(dateTimeRange, value, key);
        }
    }
}

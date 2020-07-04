using Exceptionless.DateTimeExtensions;
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

    public interface IKey<T>
    {
        T Key { get; }
    }

    public interface IDateTimeKeyPoint<T> : IDateTimePoint, IKey<T>
    {

    }

    public interface IDataPointKeyProvider<T> : IDataPointProvider, IKey<T>
    {

    }

    public struct DateTimePoint : IDateTimeKeyPoint<string>
    {

        public DateTimePoint(DateTime dateTime, double value, string key)
        {
            DateTime = dateTime;
            Value = value;
            Key = key;

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
    }

    public struct DateTimePoint<TKey> : IDateTimeKeyPoint<TKey>
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
    }

    public class DateTimeRangePoint<TKey> : IDateTimeKeyPoint<TKey>
    {

        public DateTimeRangePoint(DateTimeRange dateTimeRange, ICollection<IDateTimeKeyPoint<TKey>> value, TKey key)
        {
            DateTimeRange = dateTimeRange;
            Collection = value;
            this.Key = key;

        }

        public DateTimeRangePoint(DateTimeRange dateTimeRange, ICollection<IDateTimeKeyPoint<TKey>> value) : this(dateTimeRange, value, default)
        {
        }

        public DateTimeRange DateTimeRange { get; }

        public TKey Key { get; }

        public virtual DateTime DateTime => DateTimeRange.Start;

        public virtual double Value => Collection.First().Value;

        public ICollection<IDateTimeKeyPoint<TKey>> Collection { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(DateTime), Value);
        }

        public override string ToString()
        {
            return $"{DateTime:F}, {Value}";
        }
    }
}

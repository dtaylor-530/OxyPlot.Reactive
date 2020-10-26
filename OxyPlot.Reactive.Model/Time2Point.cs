using OxyPlot.Axes;
using System;

namespace OxyPlot.Reactive.Model
{
    public struct Time2Point<TKey> : ITime2Point<TKey>
    {
        public Time2Point(DateTime dateTime, double value, double value2, TKey key)
        {
            Var = dateTime;
            Value = value;
            Value2 = value2;
            this.Key = key;
        }

        public Time2Point(DateTime dateTime, double value, double value2) : this(dateTime, value, value2, default)
        {
        }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public double Value2 { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }

        public override string ToString() => $"{Var:F}, {Value}, {Key}";

        public static ITime2Point<TKey> Create(DateTime dateTime, double value, double value2, TKey key)
        {
            return new Time2Point<TKey>(dateTime, value, value2, key);
        }
    }

    public struct Time2Point<TKey, TValue2> : ITime2Point<TKey, TValue2>
    {
        public Time2Point(DateTime dateTime, double value, TValue2 value2, TKey key)
        {
            Var = dateTime;
            Value = value;
            Value2 = value2;
            this.Key = key;
        }

        public Time2Point(DateTime dateTime, double value, TValue2 value2) : this(dateTime, value, value2, default)
        {
        }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public TValue2 Value2 { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }

        public override string ToString() => $"{Var:F}, {Value}, {Key}";

        public static ITime2Point<TKey> Create(DateTime dateTime, double value, double value2, TKey key)
        {
            return new Time2Point<TKey>(dateTime, value, value2, key);
        }
    }

}
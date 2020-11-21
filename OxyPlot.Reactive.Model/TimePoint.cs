using System;

namespace ReactivePlot.Model
{
    public struct TimePoint : ITimePoint<string>
    {
        public TimePoint(DateTime dateTime, double value, string key)
        {
            Var = dateTime;
            Value = value;
            Key = key ?? string.Empty;
        }

        public TimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
        {
        }

        public string Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public void Deconstruct(out DateTime var, out double value)
        {
            var = Var;
            value = Value;
        }
        public override string ToString()
        {
            return $"{Var:F}, {Value}, {Key}";
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
            Var = dateTime;
            Value = value;
            this.Key = key;
        }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public void Deconstruct(out DateTime var, out double value)
        {
            var = Var;
            value = Value;
        }

        public override string ToString()
        {
            return $"{Var:F}, {Value}, {Key}";
        }

        public static ITimePoint<TKey> Create(DateTime dateTime, double value, TKey key)
        {
            return new TimePoint<TKey>(dateTime, value, key);
        }
    }
}
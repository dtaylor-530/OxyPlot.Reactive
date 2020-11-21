using System;

namespace ReactivePlot.Model
{
    public struct Point<TVar> : I2Point<string, TVar> where TVar : IComparable<TVar>
    {
        public Point(TVar dateTime, double value, string key)
        {
            Var = dateTime;
            Value = value;
            Key = key ?? string.Empty;
        }

        public Point(TVar dateTime, double value) : this(dateTime, value, default)
        {
        }

        public string Key { get; }

        public TVar Var { get; }

        public double Value { get; }

        public (double, double) DataPoint()
        {
            //return new DataPoint(Convert.ToDouble(Var), Value);
            return (Convert.ToDouble(Var), Value);
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
}
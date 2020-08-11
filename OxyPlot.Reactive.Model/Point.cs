using System;

namespace OxyPlot.Reactive.Model
{


    public struct Point<T> : I2Point<string, T>
    {

        public Point(T dateTime, double value, string key)
        {
            Var = dateTime;
            Value = value;
            Key = key ?? string.Empty;

        }

        public Point(T dateTime, double value) : this(dateTime, value, default)
        {
        }

        public string Key { get; }

        public T Var { get; }

        public double Value { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(Convert.ToDouble(Var), Value);
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

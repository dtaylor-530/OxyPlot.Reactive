using System;

namespace ReactivePlot.Model
{
    public struct TimeModelPoint<TKey, TModel> : ITimeModelPoint<TKey, TModel>
    {
        public TimeModelPoint(DateTime dateTime, double value, TModel value2, TKey key)
        {
            Var = dateTime;
            Value = value;
            Model = value2;
            this.Key = key;
        }

        public TimeModelPoint(DateTime dateTime, double value, TModel value2) : this(dateTime, value, value2, default)
        {
        }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public TModel Model { get; }

        public (double, double) DataPoint()
        {
            //return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
            return (Var.Ticks, Value);
        }

        public override string ToString() => $"{Var:F}, {Value}, {Key}";

        public static ITimeModelPoint<TKey, TModel> Create(DateTime dateTime, double value, TModel model, TKey key)
        {
            return new TimeModelPoint<TKey, TModel>(dateTime, value, model, key);
        }
    }

}
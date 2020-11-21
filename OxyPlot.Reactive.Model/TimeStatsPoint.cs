using System;

namespace ReactivePlot.Model
{
    public class TimeStatsPoint<TKey> : ITimeStatsPoint<TKey>
    {
        public TimeStatsPoint(DateTime dateTime, double value, OnTheFlyStats.Stats value2, TKey key)
        {
            Var = dateTime;
            Value = value;
            Model = value2;
            this.Key = key;
        }

        public TimeStatsPoint(DateTime dateTime, double value, OnTheFlyStats.Stats value2) : this(dateTime, value, value2, default)
        {
        }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public OnTheFlyStats.Stats Model { get; }

        //public DataPoint GetDataPoint()
        //{
        //    return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        //}
        public (double, double) DataPoint()
        {
            return (Var.Ticks, Value);
        }

        public override string ToString() => $"{Var:F}, {Value}, {Key}";

        public static ITimeStatsPoint<TKey> Create(DateTime dateTime, double value, OnTheFlyStats.Stats value2, TKey key)
        {
            return new TimeStatsPoint<TKey>(dateTime, value, value2, key);
        }
    }


    public interface ITimeStatsPoint<TKey> : ITimeModelPoint<TKey, OnTheFlyStats.Stats>
    {

    }
}

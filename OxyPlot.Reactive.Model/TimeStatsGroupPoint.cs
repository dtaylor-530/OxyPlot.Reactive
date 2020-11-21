using OnTheFlyStats;
using System;

namespace ReactivePlot.Model
{

    public struct TimeStatsGroupPoint<TKey> : ITimeStatsGroupPoint<TKey, TKey>
    {
        public TimeStatsGroupPoint(DateTime dateTime, double value, Stats model, TKey key, TKey groupKey)
        {
            Var = dateTime;
            Value = value;
            Model = model;
            Key = key;
            GroupKey = groupKey;
        }

        //public TimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
        //{
        //}

        public TKey GroupKey { get; }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public Stats Model { get; }

        public (double, double) DataPoint()
        {
            //  return (DateTimeAxis.ToDouble(Var), Value);
            return (Var.Ticks, Value);
        }

        public override string ToString()
        {
            return $"{Var:F}, {Value}, {Key}";
        }

        public static ITimeGroupPoint<TKey, TKey> Create(DateTime dateTime, double value, TKey key, TKey groupKey)
        {
            return new TimeGroupPoint<TKey>(dateTime, value, key, groupKey);
        }
    }

    public struct TimeStatsGroupPoint<TGroupKey, TKey> : ITimeStatsGroupPoint<TGroupKey, TKey>
    {
        public TimeStatsGroupPoint(DateTime dateTime, double value, Stats model, TKey key, TGroupKey groupKey)
        {
            Var = dateTime;
            Value = value;
            Key = key;
            GroupKey = groupKey;
            Model = model;
        }

        //public TimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
        //{
        //}

        public TGroupKey GroupKey { get; }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }
        public Stats Model { get; }

        //public DataPoint GetDataPoint()
        //{
        //    return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        //}


        public (double, double) DataPoint()
        {
            return (Var.Ticks, Value);
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
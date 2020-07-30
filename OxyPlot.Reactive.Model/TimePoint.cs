﻿using Itenso.TimePeriod;
using LinqStatistics;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Model
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

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
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

        public TimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
        {
        }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
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

    public class TimeRangePoint<TKey> : ITimeRangePoint<TKey>
    {
        private readonly Operation operation;

        public TimeRangePoint(ITimeRange timeRange, ICollection<ITimePoint<TKey>> value, TKey key, Operation operation)
        {
            TimeRange = timeRange;
            Collection = value;
            this.Key = key;
            this.operation = operation;
        }

        public TimeRangePoint(ITimeRange dateTimeRange, ICollection<ITimePoint<TKey>> value, TKey key) : this(dateTimeRange, value, key, Operation.Mean)
        {
        }
        public TimeRangePoint(ITimeRange dateTimeRange, ICollection<ITimePoint<TKey>> value) : this(dateTimeRange, value, default, Operation.Mean)
        {
        }
        public TKey Key { get; }

        public ITimeRange TimeRange { get; }


        public virtual DateTime Var => new DateTime((long)(TimeRange.Start.Ticks + (TimeRange.End - TimeRange.Start).Ticks / 2d));

        //     public virtual double Value => Collection.Count>1? Collection.Average(a => a.Value): Collection.First().Value;

        public virtual double Value
        {
            get
            {
                return Collection.Count > 1 ? GetValue() : Collection.Single().Value;
            }
        }

        double GetValue()
        {
            switch (this.operation)
            {
                case Operation.Mean: return Collection.Average(a => a.Value);
                case Operation.Variance: return Collection.Variance(a => a.Value);
                case Operation.StandardDeviation: return Collection.StandardDeviation(a => a.Value);
                case Operation.Mode: return Collection.Mode(a => a.Value) ?? 0;
                case Operation.Max: return Collection.Max(a => a.Value);
                case Operation.Min: return Collection.Min(a => a.Value);
                case Operation.Median: return Collection.Median(a => a.Value);
                default: throw new NotImplementedException();
            };
        }

        public ICollection<ITimePoint<TKey>> Collection { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }

        public override string ToString()
        {
            return $"{Var:F}, {Value}";
        }

        public static ITimePoint<TKey> Create(ITimeRange dateTimeRange, ICollection<ITimePoint<TKey>> value, TKey key)
        {
            return new TimeRangePoint<TKey>(dateTimeRange, value, key);
        }
    }
}

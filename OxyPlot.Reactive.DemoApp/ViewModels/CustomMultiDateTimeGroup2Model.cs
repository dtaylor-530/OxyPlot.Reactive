﻿using MoreLinq;
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using OxyPlot.Reactive.Infrastructure;
using System.Reactive;
using Itenso.TimePeriod;
using LinqStatistics;

namespace OxyPlot.Reactive.DemoApp.ViewModels
{
    public class CustomMultiDateTimeGroup2Model<TKey> : TimeGroup2Model<TKey>
    {
        public CustomMultiDateTimeGroup2Model(PlotModel model, IEqualityComparer<TKey> comparer = null, IScheduler scheduler = null) : base(model, comparer, scheduler)
        {
        }

        protected override IEnumerable<ITimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            var se = ranges != null ?

                ees.GroupOn(ranges, a => a.Value.Key)
                .Where(a => a.Any())
                .Select(ac =>
                {
                    var ss = ac.Scan(default(TimePoint<TKey>), (a, b) => new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key))
                    .Cast<ITimePoint<TKey>>()
                    .Skip(1).ToArray();
                    return new CustomDateTimeRangePoint<TKey>(ac.Key, ss);
                }).ToArray() :

                ees.Scan(default(TimePoint<TKey>), (a, b) => new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key))
                .Select(a => new CustomDateTimeRangePoint<TKey>(new Range<DateTime>(a.Var, a.Var), new ITimePoint<TKey>[] {
                    new TimePoint<TKey>(a.Var, a.Value, a.Key) }, a.Key))
                //.Cast<IDateTimePoint<TKey>>()
                .Skip(1)
                .ToArray();

            return se;
        }

        public void Refresh()
        {
            refreshSubject.OnNext(Unit.Default);
        }
    }

    public class CustomDateTimeRangePoint<TKey> : TimeRangePoint<TKey>
    {
        public CustomDateTimeRangePoint(Range<DateTime> dateTimeRange, ICollection<IPoint<DateTime, double>> value) : base(dateTimeRange, value)
        {
        }

        public CustomDateTimeRangePoint(Range<DateTime> dateTimeRange, ICollection<IPoint<DateTime, double>> value, TKey key) : base(dateTimeRange, value, key)
        {
        }

        public override double Value =>
            Collection.Count > 1 ?
              Collection.Average(a => a.Value/ (a.Var- DateTime.UnixEpoch).TotalDays)      
            : Collection.First().Value;
    }
}

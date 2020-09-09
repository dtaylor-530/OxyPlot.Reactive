﻿using MoreLinq;
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using OxyPlot.Reactive.Infrastructure;
using LinqStatistics;

namespace OxyPlot.Reactive.DemoApp.ViewModels
{
    public class CustomTimeGroup2Model<TKey> : TimeGroup2Model<TKey>
    {
        public CustomTimeGroup2Model(PlotModel model, IEqualityComparer<TKey> comparer = null, IScheduler scheduler = null) : base(model, comparer, scheduler)
        {
        }


        protected override IEnumerable<ITimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, ITimePoint<TKey>>> collection)
        {
            var ees = collection
                  .OrderBy(a => a.Value.Key);

            var se = ranges != null ? Ranges() : NoRanges();

            return se.ToArray();

            IEnumerable<ITimeRangePoint<TKey>> Ranges()
            {

                return ees
                    .GroupOn(ranges, a => a.Value.Var)
                    .Where(a => a.Any())
                    .Scan((default(TimeRangePoint<TKey>), default(ITimePoint<TKey>)), (ac, bc) =>
                    {
                        var ss = bc.Scan(ac.Item2, (a, b) => new TimePoint<TKey>(b.Value.Var, Combine(a?.Value ?? 0, b.Value.Value), b.Key))
                        .Cast<ITimePoint<TKey>>()
                        .Skip(1)
                        .ToArray();

                        return (new CustomDateTimeRangePoint<TKey>(bc.Key, ss, bc.FirstOrDefault().Key), ss.Last());
                    })
                    .Skip(1)
                    .Select(a => a.Item1)
                    .Cast<ITimeRangePoint<TKey>>();
            }

            IEnumerable<ITimeRangePoint<TKey>> NoRanges()
            {
                return ees.Scan(default(TimePoint<TKey>), (a, b) => new TimePoint<TKey>(b.Value.Var, Combine(a.Value, b.Value.Value), b.Key))
                    .Select(a => new CustomDateTimeRangePoint<TKey>(new Range<DateTime>(a.Var, a.Var), new ITimePoint<TKey>[] { a }, a.Key))
                    .Skip(1);
            }
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

using MoreLinq;
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using OxyPlot.Reactive.Infrastructure;
using System.Reactive;
using Exceptionless.DateTimeExtensions;

namespace OxyPlot.Reactive.DemoApp.ViewModels
{
    public class CustomMultiDateTimeGroup2Model<TKey> : MultiDateTimeGroup2Model<TKey>
    {
        public CustomMultiDateTimeGroup2Model(PlotModel model, IEqualityComparer<TKey> comparer = null, IScheduler scheduler = null) : base(model, comparer, scheduler)
        {
        }

        protected override IEnumerable<IDateTimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            var se = ranges != null ?

                ees.GroupOn(ranges, a => a.Value.Key)
                .Where(a => a.Any())
                .Select(ac =>
                {
                    var ss = ac.Scan(default(DateTimePoint<TKey>), (a, b) => new DateTimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key))
                    .Cast<IDateTimePoint<TKey>>()
                    .Skip(1).ToArray();
                    return new CustomDateTimeRangePoint<TKey>(ac.Key, ss);
                }).ToArray() :

                ees.Scan(default(DateTimePoint<TKey>), (a, b) => new DateTimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key))
                .Select(a => new CustomDateTimeRangePoint<TKey>(new DateTimeRange(a.DateTime, a.DateTime), new IDateTimePoint<TKey>[] {
                    new DateTimePoint<TKey>(a.DateTime, a.Value, a.Key) }, a.Key))
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

    public class CustomDateTimeRangePoint<TKey> : DateTimeRangePoint<TKey>
    {
        public CustomDateTimeRangePoint(DateTimeRange dateTimeRange, ICollection<IDateTimePoint<TKey>> value) : base(dateTimeRange, value)
        {
        }

        public CustomDateTimeRangePoint(DateTimeRange dateTimeRange, ICollection<IDateTimePoint<TKey>> value, TKey key) : base(dateTimeRange, value, key)
        {
        }

        public override double Value =>
            Collection.Count > 1 ?
              Collection.Average(a => a.Value/ (a.DateTime- DateTime.UnixEpoch).TotalDays)      
            : Collection.First().Value;
    }
}

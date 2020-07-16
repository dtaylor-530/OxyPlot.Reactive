﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System.Reactive.Concurrency;
using MoreLinq;
using Exceptionless.DateTimeExtensions;
using System.Reactive.Subjects;

namespace OxyPlot.Reactive
{




    public class MultiDateTimeAccumulatedGroupModel<TKey> : MultiDateTimeGroup2Model<TKey>
    {

        public MultiDateTimeAccumulatedGroupModel(PlotModel model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public MultiDateTimeAccumulatedGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override double Combine(double x0, double x1)
        {
            return x0 + x1;
        }

    }



    public class MultiDateTimeGroup2Model<TKey> : MultiDateTimeModel<TKey, IDateTimeRangePoint<TKey>>, IObservable<DateTimeRange[]>, IObserver<Operation>, IObserver<TimeSpan>
    {
        private readonly Subject<DateTimeRange[]> rangesSubject = new Subject<DateTimeRange[]>();
        private RangeType rangeType = RangeType.None;
        private TimeSpan? timeSpan; private Operation? operation;
        protected DateTimeRange[]? ranges;


        public MultiDateTimeGroup2Model(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }



        protected override IDateTimeRangePoint<TKey>[] Create(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> col)
        {
            return rangeType switch
            {
                RangeType.None => ToDataPoints(col).ToArray(),
                //RangeType.Count when count.HasValue => Enumerable.TakeLast(ToDataPoints(col), count.Value),
                RangeType.TimeSpan when timeSpan.HasValue => ToDataPoints(col).ToArray(),
                _ => throw new ArgumentOutOfRangeException("fdssffd")
            };
        }


        protected override async void Modify()
        {
            if (timeSpan.HasValue)
            {
                ranges = await Task.Run<DateTimeRange[]>(() =>
                {
                    return EnumerateDateTimeRanges(min, max, timeSpan.Value).ToArray<DateTimeRange>();
                });
                rangesSubject.OnNext(ranges);
            }

            static IEnumerable<DateTimeRange> EnumerateDateTimeRanges(DateTime minDateTime, DateTime maxDateTime, TimeSpan timeSpan)
            {
                var dtRange = new DateTimeRange(minDateTime, minDateTime += timeSpan);
                while (dtRange.End < maxDateTime)
                {
                    yield return dtRange = new DateTimeRange(minDateTime, minDateTime += timeSpan);
                }
            }
        }



        protected override IEnumerable<IDateTimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            var se = ranges != null ? Ranges() : NoRanges();

            return se;

            IDateTimeRangePoint<TKey>[] Ranges()
            {

                return ees
                    .GroupOn(ranges, a => a.Value.Key)
                    .Where(a => a.Any())
                .Scan((default(DateTimeRangePoint<TKey>), default(IDateTimePoint<TKey>)), (ac, bc) =>
                {
                    var ss = bc.Scan(ac.Item2, (a, b) => new DateTimePoint<TKey>(b.Value.Key, Combine(a?.Value ?? 0, b.Value.Value), b.Key))
                    .Cast<IDateTimePoint<TKey>>()
                    .Skip(1)
                    .ToArray();

                    return (new DateTimeRangePoint<TKey>(bc.Key, ss, bc.FirstOrDefault().Key, this.operation.HasValue ? operation.Value : Operation.Mean), ss.Last());
                })
                .Skip(1)
                    .Select(a=>a.Item1)
                 .Cast<IDateTimeRangePoint<TKey>>()
                 .ToArray();
            }
            IDateTimeRangePoint<TKey>[] NoRanges()
            {
                return ees.Scan(default(DateTimePoint<TKey>), (a, b) => new DateTimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key))
                    .Select(a => new DateTimeRangePoint<TKey>(new DateTimeRange(a.DateTime, a.DateTime), new IDateTimePoint<TKey>[] { a }, a.Key))
                    .Skip(1)
                    .ToArray();
            }
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            rangeType = RangeType.TimeSpan;
            refreshSubject.OnNext(Unit.Default);
        }


        public void OnNext(Operation value)
        {
            this.operation = value;
            refreshSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<DateTimeRange[]> observer)
        {
            return rangesSubject.Subscribe(observer);
        }

        enum RangeType
        {
            None,
            Count = 1,
            TimeSpan,
        }
    }
}

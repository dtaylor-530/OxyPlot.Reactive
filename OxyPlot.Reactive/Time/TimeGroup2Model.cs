#nullable enable

using MoreLinq;
using OxyPlot.Reactive.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    using DynamicData;
    using LinqStatistics;
    using Model;

    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroup2Model<TKey> : TimeGroup2Model<TKey, TKey>
    {

        public TimeGroup2Model(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }
    }

    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroup2Model<TGroupKey, TKey> : TimeModel<TGroupKey, TKey, ITimePoint<TKey>, ITimeRangePoint<TKey>>, IObserver<Operation>, IObserver<TimeSpan>, IObservable<Range<DateTime>[]>, IObservable<IChangeSet<ITimeRangePoint<TKey>>>
    {
        private readonly Subject<Range<DateTime>[]> rangesSubject = new Subject<Range<DateTime>[]>();
        private TimeSpan? timeSpan;
        private Operation? operation;
        protected Range<DateTime>[]? ranges;
        protected readonly ISubject<TimeSpan> timeSpanChanges = new Subject<TimeSpan>();
        protected readonly IObservable<IChangeSet<ITimeRangePoint<TKey>>> changeSet;

        public TimeGroup2Model(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            changeSet = ObservableChangeSet.Create<ITimeRangePoint<TKey>>(sourceList =>

            (this as IObservable<ITimeRangePoint<TKey>[]>).TakeUntil(timeSpanChanges)
            .Merge(timeSpanChanges.Select(a => this as IObservable<ITimeRangePoint<TKey>[]>).Switch())
        .Subscribe(c =>
         {
             (this as IMixedScheduler).ScheduleAction(() =>
             {
                 sourceList.Clear();
                 sourceList.AddRange(c);
             });
         }));
        }

        //protected override ITimeRangePoint<TKey>[] Create(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> col)
        //{
        //    return rangeType switch
        //    {
        //        RangeType.None => ToDataPoints(col).ToArray(),
        //        //RangeType.Count when count.HasValue => Enumerable.TakeLast(ToDataPoints(col), count.Value),
        //        RangeType.TimeSpan when timeSpan.HasValue => ToDataPoints(col).ToArray(),
        //        _ => throw new ArgumentOutOfRangeException("fdssffd")
        //    };
        //}

        protected override async void PreModify()
        {
            if (timeSpan.HasValue)
            {
                ranges = await Task.Run(() =>
                {
                    return EnumerateDateTimeRanges(Min, Max, timeSpan.Value).ToArray();
                });
                rangesSubject.OnNext(ranges);
            }

            static IEnumerable<Range<DateTime>> EnumerateDateTimeRanges(DateTime minDateTime, DateTime maxDateTime, TimeSpan timeSpan)
            {
                var dtRange = new Range<DateTime>(minDateTime, minDateTime += timeSpan);
                yield return dtRange;
                while (dtRange.Max < maxDateTime)
                {
                    yield return dtRange = new Range<DateTime>(minDateTime, minDateTime += timeSpan);
                }
            }
        }

        protected override IEnumerable<ITimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, ITimePoint<TKey>>> collection)
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
                        var ss = bc
                        .Select(a => a.Value)
                        .Scan(ac.Item2, (a, b) => CreatePoint(a, b))
                        .Cast<ITimePoint<TKey>>()
                        .Skip(1)
                        .ToArray();

                        return (new TimeRangePoint<TKey>(bc.Key, ss, bc.FirstOrDefault().Value.Key, this.operation.HasValue ? operation.Value : Operation.Mean), ss.Last());
                    })
                    .Skip(1)
                    .Select(a => a.Item1)
                    .Cast<ITimeRangePoint<TKey>>();
            }

            IEnumerable<ITimeRangePoint<TKey>> NoRanges()
            {
                return base.ToDataPoints(collection)
                    .Select(a => new TimeRangePoint<TKey>(new Range<DateTime>(a.Var, a.Var), new ITimePoint<TKey>[] { a }, a.Key));
            }
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            //rangeType = RangeType.TimeSpan;
            timeSpanChanges.OnNext(value);
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(Operation value)
        {
            this.operation = value;
            refreshSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<Range<DateTime>[]> observer)
        {
            return rangesSubject.Subscribe(observer);
        }

        public IDisposable Subscribe(IObserver<IChangeSet<ITimeRangePoint<TKey>>> observer)
        {
            return changeSet.Subscribe(observer);
        }
    }
}
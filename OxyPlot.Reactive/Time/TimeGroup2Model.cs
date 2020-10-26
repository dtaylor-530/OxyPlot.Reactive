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
    public class TimeGroup2Model<TGroupKey, TKey> : TimeGroup2Model<TGroupKey, TKey, ITimeRangePoint<TKey>>
    {

        public TimeGroup2Model(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }

        protected override ITimeRangePoint<TKey> CreatePoint(ITimeRangePoint<TKey>? timePoint0, IGrouping<Range<DateTime>, KeyValuePair<TGroupKey, ITimePoint<TKey>>> timePoints)
        {

            var points = ToDataPoints(timePoints.Select(a => a.Value), timePoint0?.Collection.Last()).ToArray();
            return new TimeRangePoint<TKey>(timePoints.Key, points, timePoints.FirstOrDefault().Value.Key, this.operation.HasValue ? operation.Value : Operation.Mean);

            IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<ITimePoint<TKey>> timePoints, ITimePoint<TKey>? timePoint0)
            {
                var ss = timePoints
                        .Scan(timePoint0, (a, b) => CreatePoint(a, b))
                        .Skip(1);

                return ss;
            }
        }

        protected override IEnumerable<ITimeRangePoint<TKey>> NoRanges(IOrderedEnumerable<KeyValuePair<TGroupKey, ITimePoint<TKey>>>? ees)
        { 
            return ees
                    .Select(a => a.Value)
                .Select(a => (ITimeRangePoint<TKey>)new TimeRangePoint<TKey>(new Range<DateTime>(a.Var, a.Var), new ITimePoint<TKey>[] { a }, a.Key));
        }
    
    }


    public abstract class TimeGroup2Model<TGroupKey, TKey, TRangePoint> : TimeGroup2Model<TGroupKey, TKey, ITimePoint<TKey>, TRangePoint>
        where TRangePoint : class, ITimeRangePoint<TKey>
    {
        public TimeGroup2Model(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

    }

    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeGroup2Model<TGroupKey, TKey, TType, TRangePoint> :
        TimeModel<TGroupKey, TKey, TType, TRangePoint>, IObserver<Operation>, IObserver<TimeSpan>, IObservable<Range<DateTime>[]>, IObservable<IChangeSet<TRangePoint>>
        where TRangePoint : class, TType, ITimeRangePoint<TKey, TType>
        where TType : ITimePoint<TKey>
    {
        protected readonly Subject<Range<DateTime>[]> rangesSubject = new Subject<Range<DateTime>[]>();
        protected TimeSpan? timeSpan;
        protected Operation? operation;
        protected Range<DateTime>[]? ranges;
        protected readonly ISubject<TimeSpan> timeSpanChanges = new Subject<TimeSpan>();
        protected readonly IObservable<IChangeSet<TRangePoint>> changeSet;

        public TimeGroup2Model(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            changeSet = ObservableChangeSet.Create<TRangePoint>(sourceList =>

           (this as IObservable<TRangePoint[]>).TakeUntil(timeSpanChanges)
           .Merge(timeSpanChanges.Select(a => this as IObservable<TRangePoint[]>).Switch())
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

        protected override IEnumerable<TRangePoint> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, TType>> collection)
        {
            IOrderedEnumerable<KeyValuePair<TGroupKey, TType>>? ees = collection
                .OrderBy(a => a.Value.Key);

            var se = ranges != null ? Ranges(ees) : NoRanges(ees);

            return se.ToArray();




        }

        protected virtual IEnumerable<TRangePoint> Ranges(IOrderedEnumerable<KeyValuePair<TGroupKey, TType>>? ees)
        {
            return ees
                .GroupOn(ranges, a => a.Value.Var)
                .Where(a => a.Any())
                .Scan(default(TRangePoint), CreatePoint)
                .Skip(1)
                .Cast<TRangePoint>();
        }

        protected abstract IEnumerable<TRangePoint> NoRanges(IOrderedEnumerable<KeyValuePair<TGroupKey, TType>>? ees);


        protected abstract TRangePoint CreatePoint(TRangePoint? timePoint0, IGrouping<Range<DateTime>, KeyValuePair<TGroupKey, TType>> timePoints);


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

        public IDisposable Subscribe(IObserver<IChangeSet<TRangePoint>> observer)
        {
            return changeSet.Subscribe(observer);
        }
    }
}
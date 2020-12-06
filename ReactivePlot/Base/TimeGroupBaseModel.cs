using DynamicData;
using LinqStatistics;
using MoreLinq;
using ReactivePlot.Common;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ReactivePlot.Base
{

    public abstract class TimeGroupBaseModel<TGroupKey, TKey, TRangePoint> : TimeGroupBaseModel<TGroupKey, TKey, ITimePoint<TKey>, TRangePoint>
           where TRangePoint : class, ITimeRangePoint<TKey>
    {
        public TimeGroupBaseModel(IMultiPlotModel<TRangePoint> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

        }
    }

    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeGroupBaseModel<TGroupKey, TKey, TType, TRangePoint> :
        TimeMinMaxModel<TGroupKey, TKey, TType, TRangePoint>,
        IObserver<Operation>, IObserver<TimeSpan>,
        IObservable<Range<DateTime>[]>,
        IObservable<IChangeSet<TRangePoint>>
        where TRangePoint : class, TType, ITimeRangePoint<TKey, TType>
        where TType : ITimePoint<TKey>
    {
        protected readonly Subject<Range<DateTime>[]> rangesSubject = new Subject<Range<DateTime>[]>();
        protected TimeSpan? timeSpan;
        protected Operation? operation;
        protected Range<DateTime>[]? ranges;
        protected readonly ISubject<TimeSpan> timeSpanChanges = new Subject<TimeSpan>();
        protected readonly IObservable<IChangeSet<TRangePoint>> changeSet;

        public TimeGroupBaseModel(IMultiPlotModel<TRangePoint> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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

        protected override IEnumerable<TRangePoint> ToDataPoints(IEnumerable<TType> collection)
        {
            IOrderedEnumerable<TType>? ees = collection
                .OrderBy(a => a.Key);

            var se = ranges != null ? Ranges(ees) : NoRanges(ees);

            return se.ToArray();
        }

        protected virtual IEnumerable<TRangePoint> Ranges(IOrderedEnumerable<TType>? ees)
        {
            return ees
                .GroupOn(ranges, a => a.Var)
                .Where(a => a.Any())
                .Scan(default(TRangePoint), CreatePoint)
                .Skip(1)
                .Cast<TRangePoint>();
        }


        protected virtual IEnumerable<TRangePoint> NoRanges(IOrderedEnumerable<TType>? ees)
        {
            return ees
                .Select(a => (range: new Range<DateTime>(a.Var, a.Var), a))
                .GroupBy(a => a.range, a => a.a)
                .Scan(default(TRangePoint), CreatePoint)
                .Skip(1)
                .Cast<TRangePoint>();
        }

        protected abstract TRangePoint CreatePoint(TRangePoint? timePoint0, IGrouping<Range<DateTime>, TType> timePoints);


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

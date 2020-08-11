#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System.Reactive.Concurrency;
using MoreLinq;
using LinqStatistics;
using DynamicData;
using System.Reactive.Subjects;

namespace OxyPlot.Reactive
{
    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupModel<TKey> : TimeModel<TKey, ITimeRangePoint<TKey>>, IObserver<TimeSpan>, IObservable<IChangeSet<ITimeRangePoint<TKey>>>
    {
        private TimeSpan? timeSpan;
        private ISubject<TimeSpan> timeSpanChanges = new Subject<TimeSpan>();
        private Operation? operation;
        private readonly IObservable<IChangeSet<ITimeRangePoint<TKey>>> changeSet;

        public TimeGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            changeSet = ObservableChangeSet.Create<ITimeRangePoint<TKey>>(sourceList =>
            {
                return timeSpanChanges.Select(a=> (this as IObservable<ITimeRangePoint<TKey>[]>))
                .Switch()
                .Subscribe(c =>
                 {
                     (this as IMixedScheduler).ScheduleAction(() =>
                     {
                         sourceList.Clear();
                         sourceList.AddRange(c);
                     });
                 });
            });
        }

        protected override IEnumerable<ITimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            if (timeSpan.HasValue)
            {
                var arr = ees.GroupOn(timeSpan.Value, a => a.Value.Key).ToArray();
                return arr.Select(ac =>
                {
                    var ss = ac.Scan(default(TimePoint<TKey>), (a, b) => new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key)).Cast<ITimePoint<TKey>>()
                    .Skip(1).ToArray();
                    return new TimeRangePoint<TKey>(ac.Key, ss, ac.FirstOrDefault().Key, this.operation.HasValue ? operation.Value : Operation.Mean);
                }).Cast<ITimeRangePoint<TKey>>().ToArray();
            }

            return ees.Scan(default(TimeRangePoint<TKey>), (a, b) => new TimeRangePoint<TKey>(new Range<DateTime>(b.Value.Key, b.Value.Key), new ITimePoint<TKey>[] { new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value)) }, b.Key))
                .Cast<ITimeRangePoint<TKey>>().Skip(1).ToArray();
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            timeSpanChanges.OnNext(value);
            // rangeType = RangeType.TimeSpan;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(Operation value)
        {
            operation = value;
            refreshSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<IChangeSet<ITimeRangePoint<TKey>>> observer)
        {
            return changeSet.Subscribe(observer);
        }
    }
}

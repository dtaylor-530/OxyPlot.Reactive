#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OxyPlot.Reactive.Infrastructure;
using System.Reactive.Concurrency;
using MoreLinq;
using System.Reactive.Subjects;

namespace OxyPlot.Reactive
{
    using Itenso.TimePeriod;
    using Model;


    public class TimeGroup2Model<TKey> : TimeModel<TKey, ITimePoint<TKey>, ITimeRangePoint<TKey>>, IObservable<ITimeRange[]>, IObserver<Operation>, IObserver<TimeSpan>
    {
        private readonly Subject<ITimeRange[]> rangesSubject = new Subject<ITimeRange[]>();
        //private RangeType rangeType = RangeType.None;
        private TimeSpan? timeSpan; 
        private Operation? operation;
        protected ITimeRange[]? ranges;


        public TimeGroup2Model(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {

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

            static IEnumerable<ITimeRange> EnumerateDateTimeRanges(DateTime minDateTime, DateTime maxDateTime, TimeSpan timeSpan)
            {
                var dtRange = new TimeRange(minDateTime, minDateTime += timeSpan);
                while (dtRange.End < maxDateTime)
                {
                    yield return dtRange = new TimeRange(minDateTime, minDateTime += timeSpan);
                }
            }
        }



        protected override IEnumerable<ITimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            var se = ranges != null ? Ranges() : NoRanges();

            return se.ToArray();

            IEnumerable<ITimeRangePoint<TKey>> Ranges()
            {

                return ees
                    .GroupOn(ranges, a => a.Value.Key)
                    .Where(a => a.Any())
                    .Scan((default(TimeRangePoint<TKey>), default(ITimePoint<TKey>)), (ac, bc) =>
                    {
                        var ss = bc.Scan(ac.Item2, (a, b) => new TimePoint<TKey>(b.Value.Key, Combine(a?.Value ?? 0, b.Value.Value), b.Key))
                        .Cast<ITimePoint<TKey>>()
                        .Skip(1)
                        .ToArray();

                        return (new TimeRangePoint<TKey>(bc.Key, ss, bc.FirstOrDefault().Key, this.operation.HasValue ? operation.Value : Operation.Mean), ss.Last());
                    })
                    .Skip(1)
                    .Select(a => a.Item1)
                    .Cast<ITimeRangePoint<TKey>>();
            }

            IEnumerable<ITimeRangePoint<TKey>> NoRanges()
            {
                return ees.Scan(default(TimePoint<TKey>), (a, b) => new TimePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key))
                    .Select(a => new TimeRangePoint<TKey>(new TimeRange(a.Var, a.Var), new ITimePoint<TKey>[] { a }, a.Key))
                    .Skip(1);
            }
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            //rangeType = RangeType.TimeSpan;
            refreshSubject.OnNext(Unit.Default);
        }


        public void OnNext(Operation value)
        {
            this.operation = value;
            refreshSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<ITimeRange[]> observer)
        {
            return rangesSubject.Subscribe(observer);
        }
    }
}

﻿#nullable enable

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
    using LinqStatistics;
    using Model;


    public class CartesianGroup2Model<TKey> : CartesianModel<TKey, IDoublePoint<TKey>, IDoubleRangePoint<TKey>>, IObservable<(double size, Range<double>[] ranges)>, IObserver<Operation>, IObserver<int>
    {
        private readonly Subject<(double, Range<double>[])> rangesSubject = new Subject<(double, Range<double>[])>();
        private double? span;
        private Operation? operation;
        protected Range<double>[]? ranges;


        public CartesianGroup2Model(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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
            if (span.HasValue)
            {
                ranges = await Task.Run(() =>
                {
                    return EnumerateRanges(Min, Max, span.Value).ToArray();
                });
                rangesSubject.OnNext((span.Value, ranges));
            }

            static IEnumerable<Range<double>> EnumerateRanges(double min, double max, double span)
            {
                var range = new Range<double>(min, min += span);
                while (range.Max < max)
                {
                    yield return range = new Range<double>(min, min += span);
                }
            }
        }



        protected override IEnumerable<IDoubleRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<double, double>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            var se = ranges != null ? Ranges() : NoRanges();

            return se.ToArray();

            IEnumerable<IDoubleRangePoint<TKey>> Ranges()
            {

                return ees
                    .GroupOn(ranges, a => a.Value.Key)
                    .Where(a => a.Any())
                    .Scan((default(DoubleRangePoint<TKey>), default(IDoublePoint<TKey>)), (ac, bc) =>
                    {
                        var ss = bc.Scan(ac.Item2, (a, b) => new DoublePoint<TKey>(b.Value.Key, Combine(a?.Value ?? 0, b.Value.Value), b.Key))
                        .Cast<IDoublePoint<TKey>>()
                        .Skip(1)
                        .ToArray();

                        return (new DoubleRangePoint<TKey>(bc.Key, ss, bc.FirstOrDefault().Key, this.operation.HasValue ? operation.Value : Operation.Mean), ss.Last());
                    })
                    .Skip(1)
                    .Select(a => a.Item1)
                    .Cast<IDoubleRangePoint<TKey>>();
            }

            IEnumerable<IDoubleRangePoint<TKey>> NoRanges()
            {
                return ees.Scan(default(DoublePoint<TKey>), (a, b) => new DoublePoint<TKey>(b.Value.Key, Combine(a.Value, b.Value.Value), b.Key))
                    .Select(a => new DoubleRangePoint<TKey>(new Range<double>(a.Var, a.Var), new IDoublePoint<TKey>[] { a }, a.Key))
                    .Skip(1);
            }
        }

        public void OnNext(double value)
        {
            span = value;
            refreshSubject.OnNext(Unit.Default);
        }


        public void OnNext(Operation value)
        {
            this.operation = value;
            refreshSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<(double size, Range<double>[] ranges)> observer)
        {
            var disposable = rangesSubject.Subscribe(observer);
            refreshSubject.OnNext(Unit.Default);
            return disposable;
        }
    }
}

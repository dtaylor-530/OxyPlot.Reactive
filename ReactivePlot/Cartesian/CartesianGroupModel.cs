#nullable enable

using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ReactivePlot.Cartesian
{
    using LinqStatistics;
    using Model;
    using ReactivePlot.Base;
    using ReactivePlot.Common;
    using ReactivePlot.Model.Enum;

    public class CartesianGroupModel<TKey> : CartesianModel<TKey, IDoublePoint<TKey>, IDoubleRangePoint<TKey>>, IObservable<(double size, Range<double>[] ranges)>, IObserver<Operation>, IObserver<int>
    {
        private readonly Subject<(double, Range<double>[])> rangesSubject = new Subject<(double, Range<double>[])>();
        private double? span;
        private Operation? operation;
        protected Range<double>[]? ranges;

        public CartesianGroupModel(IPlotModel<IDoubleRangePoint<TKey>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }


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
                yield return range;

                while (range.Max < max)
                {
                    yield return range = new Range<double>(min, min += span);
                }
            }
        }

        protected override IEnumerable<IDoubleRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, IDoublePoint<TKey>>> collection)
        {
            var ees = collection
                .OrderBy(a => a.Value.Key);

            var se = (ranges != null) ? Ranges() : NoRanges();

            return se.ToArray();

            IEnumerable<IDoubleRangePoint<TKey>> Ranges()
            {
                var se = ees
                    .GroupOn(ranges, a => a.Value.Var)
                    .Where(a => a.Any())
                    .Scan(default(DoubleRangePoint<TKey>), (ac, bc) =>
                    {
                        var ss = bc
                        .Select(a => a.Value)
                        .Scan(ac?.Collection.Last(), (a, b) => CreatePoint(a, b))
                        .Cast<IDoublePoint<TKey>>()
                        .Skip(1)
                        .ToArray();

                        return new Model.DoubleRangePoint<TKey>(bc.Key, ss, bc.FirstOrDefault().Key, this.operation.HasValue ? operation.Value : Operation.Mean);
                    })
                    .Skip(1)
                    .Cast<IDoubleRangePoint<TKey>>();

                return se;
            }

            IEnumerable<IDoubleRangePoint<TKey>> NoRanges()
            {
                return ees
            .Select(a => a.Value)
            .Select(a => { return a; })
            .Scan(seed: default(IDoublePoint<TKey>), (a, b) => CreatePoint(a, b))
            .Skip(1)
            .Select(a => new DoubleRangePoint<TKey>(new Range<double>(a.Var, a.Var), new IDoublePoint<TKey>[] { a }, a.Key));
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

        protected override IDoublePoint<TKey> CreatePoint(IDoublePoint<TKey> xy0, IDoublePoint<TKey> xy)
        {
            return new DoublePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }
    }
}
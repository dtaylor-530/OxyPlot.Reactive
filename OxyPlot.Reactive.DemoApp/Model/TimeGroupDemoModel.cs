#nullable enable

using DynamicData;
using LinqStatistics;
using MoreLinq;
using ReactivePlot.Base;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactivePlot.Common;
using ReactivePlot.Model.Enum;
using ReactivePlot.OxyPlot.PlotModel;

namespace OxyPlot.Reactive.DemoApp.Model
{
    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeGroupDemoModel<TKey> : TimeModel<TKey, ITimePoint<TKey>, ITimeRangePoint<TKey>>, IObserver<TimeSpan>
    {
        private TimeSpan? timeSpan;
        private Operation? operation;

        public TimeGroupDemoModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : 
            base(new OxyTimePlotModel<TKey, ITimeRangePoint<TKey>>(model), comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<ITimeRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, ITimePoint<TKey>>> collection)
        {
            var ordered = collection
                .OrderBy(a => a.Value.Key);

            if (timeSpan.HasValue)
            {
                var arr = ordered.GroupOn(timeSpan.Value, a => a.Value.Var).ToArray();
                return arr.Select(ac =>
                {
                    var ss = ac
                    .Select(a => a.Value)
                    .Scan(default(ITimePoint<TKey>), (a, b) => CreatePoint(a, b))
                    .Cast<ITimePoint<TKey>>()
                    .Skip(1)
                    .ToArray();
                    return new TimeRangePoint<TKey>(ac.Key, ss, ac.FirstOrDefault().Key, operation.HasValue ? operation.Value : Operation.Mean);
                }).Cast<ITimeRangePoint<TKey>>().ToArray();
            }

            return ordered
                 .Select(a => a.Value)
                .Scan(default(TimeRangePoint<TKey>), (a, b) => new TimeRangePoint<TKey>(new Range<DateTime>(b.Var, b.Var), new ITimePoint<TKey>[] { CreatePoint(a, b) }, b.Key))
                .Cast<ITimeRangePoint<TKey>>()
                .Skip(1)
                .ToArray();
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(Operation value)
        {
            operation = value;
            refreshSubject.OnNext(Unit.Default);
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }
    }
}
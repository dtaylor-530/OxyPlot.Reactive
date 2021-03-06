﻿#nullable enable
using Endless;
using OxyPlot.Reactive.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{
    public abstract class TimeModelPointModel<TKey, R, Y> : TimeModel<TKey, R, R> where R : ITimeModelPoint<TKey, Y>
    {
        public TimeModelPointModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<R> ToDataPoints(IEnumerable<KeyValuePair<TKey, R>> collection)
        {
            return
                collection
                .Select(a => a.Value)
                .Select(a => { return a; })
                .Scan(seed: default(R), (a, b) => CreatePoint(a, b))
                .Skip(1);
        }


    }

}

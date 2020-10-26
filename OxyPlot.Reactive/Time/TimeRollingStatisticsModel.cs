#nullable enable
using Endless;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive.Time
{

    public class TimeRollingStatisticsModel : TimeRollingStatisticsModel<string>
    {
        public TimeRollingStatisticsModel(PlotModel model) : base(model)
        {
        }
    }

    //public class TimeRollingStatisticsModel : TimeRollingStatisticsModel<string, st>
    //{
    //    public TimeRollingStatisticsModel(PlotModel model) : base(model)
    //    {
    //    }
    //}

    public class TimeRollingStatisticsModel<TKey> : Time2Model<TKey, ITime2Point<TKey, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>
    {
        private RollingOperation operation;

        public TimeRollingStatisticsModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITime2Point<TKey, OnTheFlyStats.Stats> CreatePoint(ITime2Point<TKey, OnTheFlyStats.Stats> xy0, ITime2Point<TKey, OnTheFlyStats.Stats> xy)
        {
            return OnTheFlyStatsHelper.Combine(xy0, xy, operation);
        }

        public void OnNext(RollingOperation operation)
        {
            this.operation = operation;
            Refresh(new[] { Unit.Default });
        }
    }

    public abstract class Time2Model<TKey, R, Y> : TimeModel<TKey, R, R> where R : ITime2Point<TKey, Y>
    {
        public Time2Model(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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

#nullable enable
using LinqStatistics;
using MoreLinq;
using OxyPlot.Reactive.Base;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Model.Enum;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{

    public class TimeOnTheFlyStatsModel : TimeOnTheFlyStatsModel<string>
    {
        public TimeOnTheFlyStatsModel(PlotModel model, RollingOperation rollingOperation) : this(model)
        {
            this.OnNext(rollingOperation);
        }

        public TimeOnTheFlyStatsModel(PlotModel model) : base(model)
        {
        }
    }


    public class TimeOnTheFlyStatsModel<TKey> : TimeModelPointModel<TKey, ITimeModelPoint<TKey, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>
    {
        private RollingOperation operation;

        public TimeOnTheFlyStatsModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimeModelPoint<TKey, OnTheFlyStats.Stats> CreatePoint(ITimeModelPoint<TKey, OnTheFlyStats.Stats> xy0, ITimeModelPoint<TKey, OnTheFlyStats.Stats> xy)
        {
            return OnTheFlyStatsHelper.Combine(xy0, xy, operation);
        }

        public void OnNext(RollingOperation operation)
        {
            this.operation = operation;
            Refresh(new[] { Unit.Default });
        }
    }


    public class TimeOnTheFlyStatsGroupModel<TGroupKey, TKey> : TimeGroupBaseModel<TGroupKey, TKey, ITimeStatsPoint<TKey>, ITimeStatsRangePoint<TKey>>, IObserver<RollingOperation>
    {
        private RollingOperation rollingOperation;

        public TimeOnTheFlyStatsGroupModel(PlotModel model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public TimeOnTheFlyStatsGroupModel(PlotModel model, IEqualityComparer<TGroupKey>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        public void OnNext(RollingOperation value)
        {
            rollingOperation = value;
        }

        protected override ITimeStatsPoint<TKey> CreatePoint(ITimeStatsPoint<TKey> xy0, ITimeStatsPoint<TKey> xy)
        {
            return OnTheFlyStatsHelper.Combine(xy0, xy, rollingOperation);
        }

        protected override ITimeStatsRangePoint<TKey> CreatePoint(ITimeStatsRangePoint<TKey>? timePoint0, IGrouping<Range<DateTime>, KeyValuePair<TGroupKey, ITimeStatsPoint<TKey>>> timePoints)
        {

            var points = ToDataPoints(timePoints.Select(a => a.Value), timePoint0?.Collection.Last()).ToArray();
            var first = timePoints.FirstOrDefault();
            return new TimeStatsRangePoint<TKey>(timePoints.Key, points, first.Value.Model, first.Value.Key, this.operation.HasValue ? operation.Value : Operation.Mean);

            IEnumerable<ITimeStatsPoint<TKey>> ToDataPoints(IEnumerable<ITimeStatsPoint<TKey>> timePoints, ITimeStatsPoint<TKey>? timePoint0)
            {
                var ss = timePoints
                        .Scan(timePoint0, (a, b) => CreatePoint(a, b))
                        .Skip(1);

                return ss;
            }
        }

       // protected override IEnumerable<ITimeStatsRangePoint<TKey>> NoRanges(IOrderedEnumerable<KeyValuePair<TGroupKey, ITimeStatsPoint<TKey>>>? ees)
        //{
        //    var stats = new OnTheFlyStats.Stats();
        //    return ees
        //            .Select(a => a.Value)
        //        .Scan(default(ITimeStatsRangePoint<TKey>), (a, b) =>
        //         new TimeStatsRangePoint<TKey>(new Range<DateTime>(b.Var, b.Var), new ITimeStatsPoint<TKey>[] { CreatePoint(a, b) }, stats, b.Key))
        //        .Skip(1);
        //}
    }





}

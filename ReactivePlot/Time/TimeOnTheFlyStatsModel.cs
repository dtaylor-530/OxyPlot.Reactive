#nullable enable
using LinqStatistics;
using MoreLinq;
using OnTheFlyStats;
using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace ReactivePlot.Time
{

    public class TimeOnTheFlyStatsModel : TimeOnTheFlyStatsModel<string>
    {
        public TimeOnTheFlyStatsModel(IMultiPlotModel<ITimeModelPoint<string, OnTheFlyStats.Stats>> model, RollingOperation rollingOperation) : this(model)
        {
            this.OnNext(rollingOperation);
        }

        public TimeOnTheFlyStatsModel(IMultiPlotModel<ITimeModelPoint<string, OnTheFlyStats.Stats>> model) : base(model)
        {
        }
    }


    public class TimeOnTheFlyStatsModel<TKey> : TimeModelPointModel<TKey, ITimeModelPoint<TKey, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>
    {
        private RollingOperation operation;

        public TimeOnTheFlyStatsModel(IMultiPlotModel<ITimeModelPoint<TKey, OnTheFlyStats.Stats>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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

        protected override TKey GetKey(ITimeModelPoint<TKey, Stats> item)
        {
            throw new NotImplementedException();
        }
    }


    public class TimeOnTheFlyStatsGroupModel<TGroupKey, TKey> :
        TimeGroupBaseModel<TGroupKey, TKey, ITimeStatsPoint<TKey>, ITimeStatsRangePoint<TKey>>,
        IObserver<RollingOperation>
    {
        private RollingOperation rollingOperation;

        public TimeOnTheFlyStatsGroupModel(IMultiPlotModel<ITimeStatsRangePoint<TKey>> model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public TimeOnTheFlyStatsGroupModel(IMultiPlotModel<ITimeStatsRangePoint<TKey>> model, IEqualityComparer<TGroupKey>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        public void OnNext(RollingOperation value)
        {
            rollingOperation = value;
        }

        protected override ITimeStatsRangePoint<TKey> CreateNewPoint(ITimeStatsRangePoint<TKey> xy0, ITimeStatsPoint<TKey> xy)
        {
            throw new NotImplementedException();
        }

        protected override ITimeStatsPoint<TKey> CreatePoint(ITimeStatsPoint<TKey> xy0, ITimeStatsPoint<TKey> xy)
        {
            return OnTheFlyStatsHelper.Combine(xy0, xy, rollingOperation);
        }

        protected override ITimeStatsRangePoint<TKey> CreatePoint(ITimeStatsRangePoint<TKey>? timePoint0, IGrouping<Range<DateTime>, ITimeStatsPoint<TKey>> timePoints)
        {

            var points = ToDataPoints(timePoints, timePoint0?.Collection.Last()).ToArray();
            var first = timePoints.FirstOrDefault();
            return new TimeStatsRangePoint<TKey>(timePoints.Key, points, first.Model, first.Key, this.operation.HasValue ? operation.Value : Operation.Mean);

            IEnumerable<ITimeStatsPoint<TKey>> ToDataPoints(IEnumerable<ITimeStatsPoint<TKey>> timePoints, ITimeStatsPoint<TKey>? timePoint0)
            {
                var ss = timePoints
                        .Scan(timePoint0, (a, b) => CreatePoint(a, b))
                        .Skip(1);

                return ss;
            }
        }

        protected override TGroupKey GetKey(ITimeStatsPoint<TKey> item)
        {
            throw new NotImplementedException();
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

#nullable enable

using MoreLinq;
using OxyPlot.Reactive.Common;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{
    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeLogGroupStatsKeyModel : TimeLogGroupStatsModel<double>
    {
 
        public TimeLogGroupStatsKeyModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        public TimeLogGroupStatsKeyModel(PlotModel model, RollingOperation rollingOperation, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            this.OnNext(rollingOperation);
        }
        protected override string CreateGroupKey(IKeyPoint<double, DateTime, double> val)
        {
            return Power.HasValue == false ? 
                default(double).ToString() : 
                GroupKeyFactory.Create(val.Key, Power.Value);
        }
    }


    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeLogGroupStatsValueModel : TimeLogGroupStatsModel<string>
    {

        public TimeLogGroupStatsValueModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override string CreateGroupKey(IKeyPoint<string, DateTime, double> val)
        {
            return Power.HasValue == false ?
                default(double).ToString() :
                GroupKeyFactory.Create(val.Value, Power.Value);
        }
    }


    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeLogGroupStatsModel<TKey> : TimeLogGroupValueModel<TKey, ITimeStatsPoint<TKey>>, IObserver<RollingOperation>
    {
        private RollingOperation rollingOperation;

        public TimeLogGroupStatsModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        public void OnNext(RollingOperation value)
        {
            rollingOperation = value;
            refreshSubject.OnNext(Unit.Default);
        }

        protected override string CreateGroupKey(IKeyPoint<TKey, DateTime, double> val)
        {
            return Power.HasValue == false ?
                default(double).ToString() :
                GroupKeyFactory.Create(val.Value, Power.Value);
        }

        protected override ITimeStatsPoint<TKey> CreatePoint(ITimeStatsPoint<TKey> xy0, ITimeStatsPoint<TKey> xy)
        {
            return OnTheFlyStatsHelper.Combine(xy0, xy, rollingOperation);
        }

        protected override IEnumerable<ITimeStatsPoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<string, ITimeStatsPoint<TKey>>> collection)
        {
            return
            collection
            .Select(a => a.Value)
            .Select(a => { return a; })
            .Scan(seed: default(ITimeStatsPoint<TKey>), (a, b) => CreatePoint(a, b))
            .Cast<ITimeStatsPoint<TKey>>()
            .Skip(1);
        }
    }
}
#nullable enable

using MoreLinq;
using ReactivePlot.Common;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using ReactivePlot.Multi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace ReactivePlot.Time
{

    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeLogGroupStats2KeyModel : TimeLogGroupStatsModel<double, ITimeStatsGroupPoint<string, double>>
    {

        public TimeLogGroupStats2KeyModel(IPlotModel<ITimeStatsGroupPoint<string, double>> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        public TimeLogGroupStats2KeyModel(IPlotModel<ITimeStatsGroupPoint<string, double>> model, RollingOperation rollingOperation, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            this.OnNext(rollingOperation);
        }
        protected override string CreateGroupKey(IKeyPoint<double, DateTime, double> val)
        {
            return Power.HasValue == false ?
                default(double).ToString() :
                GroupKeyFactory.Create(val.Key, Power.Value);
        }

        protected override ITimeStatsGroupPoint<string, double> CreatePoint(ITimeStatsGroupPoint<string, double> xy0, ITimeStatsGroupPoint<string, double> xy)
        { 
            return OnTheFlyStatsHelper.Combine(xy0, xy, rollingOperation);
        }
    
        protected override IEnumerable<ITimeStatsGroupPoint<string, double>> ToDataPoints(IEnumerable<KeyValuePair<string, ITimeStatsGroupPoint<string, double>>> collection)
        {
            return
           collection
           .Select(a => a.Value)
           .Select(a => { return a; })
           .Scan(seed: default(ITimeStatsGroupPoint<string, double>), (a, b) => CreatePoint(a, b))
           .Cast<ITimeStatsGroupPoint<string, double>>()
           .Skip(1);
        }

    }

    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeLogGroupStatsKeyModel : TimeLogGroupStatsModel<double>
    {

        public TimeLogGroupStatsKeyModel(IPlotModel<ITimeStatsPoint<double>> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        public TimeLogGroupStatsKeyModel(IPlotModel<ITimeStatsPoint<double>> model, RollingOperation rollingOperation, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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

        public TimeLogGroupStatsValueModel(IPlotModel<ITimeStatsPoint<string>> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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
    public class TimeLogGroupStatsModel<TKey> : TimeLogGroupStatsModel<TKey, ITimeStatsPoint<TKey>>
    {
        protected RollingOperation rollingOperation;

        public TimeLogGroupStatsModel(IPlotModel<ITimeStatsPoint<TKey>> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
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


    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeLogGroupStatsModel<TKey, TTimeStatsPoint> : TimeLogGroupValueModel<TKey, TTimeStatsPoint>, IObserver<RollingOperation>
        where TTimeStatsPoint : ITimeStatsPoint<TKey>
    {
        protected RollingOperation rollingOperation;

        public TimeLogGroupStatsModel(IPlotModel<TTimeStatsPoint> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
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


    }
}
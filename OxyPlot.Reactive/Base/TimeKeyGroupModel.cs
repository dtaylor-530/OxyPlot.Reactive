#nullable enable

using Endless;
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{

    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeKeyGroupModel<TKey> : TimeKeyGroupModel<TKey, TKey>
    {
        public TimeKeyGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
    }

    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeKeyGroupModel<TGroupKey, TKey> : TimeKeyGroupModel<TGroupKey, TKey, ITimePoint<TKey>, ITimePoint<TKey>>
    {
        public TimeKeyGroupModel(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

        protected override IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, ITimePoint<TKey>>> collection)
        {
            return collection
       .Select(a => a.Value)
       .Select(a => { return a; })
       .Scan(seed: default(ITimePoint<TKey>), (a, b) => CreatePoint(a, b))
       .Skip(1)
       .Cast<ITimePoint<TKey>>();
        }
    }


    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeKeyGroupModel<TGroupKey, TKey, TPointIn, TPointOut> : TimeModel<TGroupKey, TKey, TPointIn, TPointOut>
        where TPointIn : ITimePoint<TKey>
        where TPointOut : ITimePoint<TKey>, TPointIn
    {
        public TimeKeyGroupModel(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }


        protected override void PreModify()
        {
            lock (DataPoints)
            {
                var dataPoints = DataPoints.SelectMany(a => a.Value).GroupBy(a => CreateGroupKey(a)).ToArray();
                DataPoints.Clear();
                foreach (var points in dataPoints)
                {
                    DataPoints[points.Key] = CreateCollection();

                    foreach (var point in points)
                    {
                        DataPoints[points.Key].Add(point);
                    }
                }
            }
        }


        public override void OnNext(KeyValuePair<TGroupKey, TPointIn> item)
        {
            lock (temporaryCollection)
                temporaryCollection.Add(KeyValuePair.Create(CreateGroupKey(item.Value), CreatePoint(default, item.Value)));

            refreshSubject.OnNext(Unit.Default);
        }


        protected abstract TGroupKey CreateGroupKey(IKeyPoint<TKey, DateTime, double> val);

    }
}

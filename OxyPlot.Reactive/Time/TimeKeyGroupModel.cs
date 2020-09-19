#nullable enable

using OxyPlot.Reactive.Model;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{

    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeKeyGroupModel<TKey> : TimeGroupKeyModel<TKey, TKey>
    {
        public TimeKeyGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
    }

    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeKeyGroupModel<TGroupKey, TKey> : TimeModel<TGroupKey, TKey, ITimePoint<TKey>, ITimePoint<TKey>>
    {
        public TimeKeyGroupModel(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

        protected abstract TGroupKey CreateGroupKey(ITimePoint<TKey> val);

        public override void OnNext(KeyValuePair<TGroupKey, ITimePoint<TKey>> item)
        {
            lock (temporaryCollection)
                temporaryCollection.Add(KeyValuePair.Create(CreateGroupKey(item.Value), CreatePoint(null, item.Value)));

            refreshSubject.OnNext(Unit.Default);
        }
    }
}
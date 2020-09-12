#nullable enable

using OxyPlot.Reactive.Model;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{

    //public abstract class TimeKeyGroupModel<TKey> : TimeModel<TKey, TKey, ITimePoint<TKey>>
    //{
    //    public TimeKeyGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
    //    {
    //    }
    //}


    /// <summary>
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeKeyGroupModel< TKey> : TimeModel<TKey, ITimePoint<TKey>>
    {


        public TimeKeyGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        //protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        //{
        //    return new TimePoint<TKey>(xy.Var, xy.Value, CreateGroupKey(xy));

        //}
        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

        protected abstract TKey CreateGroupKey(ITimePoint<TKey> val);

        public override void OnNext(KeyValuePair<TKey, ITimePoint<TKey>> item)
        {
            lock (list)
                list.Add(KeyValuePair.Create(item.Key, CreatePoint(null, item.Value)));

            refreshSubject.OnNext(Unit.Default);
        }
    }
}
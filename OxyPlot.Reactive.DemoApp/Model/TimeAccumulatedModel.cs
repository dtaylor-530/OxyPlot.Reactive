#nullable enable

namespace OxyPlot.Reactive
{
    using OxyPlot;
    using OxyPlot.Reactive.Model;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    public class TimeAccumulatedModel<TKey> : TimeModel<TKey>
    {
        public TimeAccumulatedModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key);
        }
    }

    //public class TestTimeGroupKeyModel<TGroupKey, TKey> : TimeGroupKeyModel<TGroupKey, TKey, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>>
    //{
    //    public TestTimeGroupKeyModel(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
    //    {
    //    }

    //    protected override ITimeGroupPoint<TGroupKey, TKey> CreatePoint(ITimeGroupPoint<TGroupKey, TKey> xy0, ITimeGroupPoint<TGroupKey, TKey> xy)
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    protected override IEnumerable<ITimeGroupPoint<TGroupKey, TKey>> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, ITimeGroupPoint<TGroupKey, TKey>>> collection)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}

    public class TimeAccumulatedModel<TGroupKey, TKey> : TimeGroupKeyModel<TGroupKey, TKey>
    {

        public TimeAccumulatedModel(PlotModel model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimeGroupPoint<TGroupKey, TKey> CreatePoint(ITimeGroupPoint<TGroupKey, TKey> xy0, ITimeGroupPoint<TGroupKey, TKey> xy)
        {
            return new TimeGroupPoint<TGroupKey, TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key, xy.GroupKey);
        }
    }
}
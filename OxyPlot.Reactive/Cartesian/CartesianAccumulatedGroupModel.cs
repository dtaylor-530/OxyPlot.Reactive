#nullable enable

using OxyPlot.Reactive.Model;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{
    public class CartesianAccumulatedGroupModel<TKey> : TimeGroup2Model<TKey>
    {
        public CartesianAccumulatedGroupModel(PlotModel model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public CartesianAccumulatedGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, (xy0?.Value ?? 0 ) + xy.Value, xy.Key);
        }
    }
}
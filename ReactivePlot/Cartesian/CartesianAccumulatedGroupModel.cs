#nullable enable

using ReactivePlot.Model;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace ReactivePlot.Cartesian
{
    public class CartesianAccumulatedGroupModel<TKey> : CartesianGroupModel<TKey>
    {
        public CartesianAccumulatedGroupModel(IMultiPlotModel<IDoubleRangePoint<TKey>> model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public CartesianAccumulatedGroupModel(IMultiPlotModel<IDoubleRangePoint<TKey>> model, IEqualityComparer<TKey>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IDoublePoint<TKey> CreatePoint(IDoublePoint<TKey> xy0, IDoublePoint<TKey> xy)
        {
            return new DoublePoint<TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key);
        }
    }
}
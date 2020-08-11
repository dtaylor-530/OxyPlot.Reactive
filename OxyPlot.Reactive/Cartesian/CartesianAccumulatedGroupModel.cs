#nullable enable

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

        protected override double Combine(double x0, double x1)
        {
            return x0 + x1;
        }

    }
}

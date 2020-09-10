#nullable enable

using OxyPlot.Reactive.Model;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{
    public class TimeAccumulatedGroupModel<TKey> : TimeGroup2Model<TKey>
    {
        public TimeAccumulatedGroupModel(PlotModel model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public TimeAccumulatedGroupModel(PlotModel model, IEqualityComparer<TKey>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        //protected override double Combine(double x0, double x1)
        //{
        //    return x0 + x1;
        //}

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key);
        }

    }
}
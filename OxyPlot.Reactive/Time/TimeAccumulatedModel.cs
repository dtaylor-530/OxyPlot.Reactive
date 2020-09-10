#nullable enable

namespace OxyPlot.Reactive
{
    using OxyPlot;
    using OxyPlot.Reactive.Model;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    public class TimeAccumulatedModel<TKey> : TimeModel<TKey>
    {
        public TimeAccumulatedModel(PlotModel model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public TimeAccumulatedModel(PlotModel model, IEqualityComparer<TKey>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return (ITimePoint<TKey>)new TimePoint<TKey>(xy.Var, xy0?.Value ?? 0 + xy.Value, xy.Key);
        }

        //protected override double Combine(double x0, double x1)
        //{
        //    return x0 + x1;
        //}
    }
}
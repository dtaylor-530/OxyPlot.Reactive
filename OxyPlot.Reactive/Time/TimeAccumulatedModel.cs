#nullable enable

namespace OxyPlot.Reactive
{
    using OxyPlot;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    public class TimeAccumulatedModel<T> : TimeModel<T>
    {
        public TimeAccumulatedModel(PlotModel model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public TimeAccumulatedModel(PlotModel model, IEqualityComparer<T>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override double Combine(double x0, double x1)
        {
            return x0 + x1;
        }
    }
}
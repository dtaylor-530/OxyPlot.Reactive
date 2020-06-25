#nullable enable
namespace OxyPlot.Reactive
{
    using OxyPlot;
    using OxyPlot.Reactive.Infrastructure;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    public class MultiDateTimeAccumulatedModel<T> : MultiDateTimeModel<T>
    {

        public MultiDateTimeAccumulatedModel(PlotModel model, IScheduler? scheduler=null) : base( model,scheduler: scheduler)
        {
        }

        public MultiDateTimeAccumulatedModel(PlotModel model, IEqualityComparer<T> comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override double Combine(double x0, double x1)
        {
            return x0 + x1;
        }

    }


}



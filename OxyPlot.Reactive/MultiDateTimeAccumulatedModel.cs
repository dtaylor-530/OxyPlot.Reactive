namespace OxyPlot.Reactive
{
    using OxyPlot;
    using OxyPlot.Reactive.Infrastructure;
    using System.Collections.Generic;

    public class MultiDateTimeAccumulatedModel<T> : MultiDateTimeModel<T>
    {

        public MultiDateTimeAccumulatedModel(IDispatcher dispatcher, PlotModel model) : base(dispatcher, model)
        {
        }

        public MultiDateTimeAccumulatedModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : base(dispatcher, model, comparer)
        {
        }

        protected override double Combine(double x0, double x1)
        {
            return x0 + x1;
        }

    }


}



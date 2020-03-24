
namespace OxyPlotEx.ViewModel
{
    using OxyPlot;
    using System.Collections.Generic;

    public class MultiLineModelAccumulated<T> :MultiLineModel<T>
    {

        public MultiLineModelAccumulated(IDispatcher dispatcher, PlotModel model):base(dispatcher, model)
        {
        }

        public MultiLineModelAccumulated(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : base(dispatcher, model, comparer)
        {
        }

        protected override double Combine(double x0, double x1)
        {
            return x0 + x1;
        }

    }


}



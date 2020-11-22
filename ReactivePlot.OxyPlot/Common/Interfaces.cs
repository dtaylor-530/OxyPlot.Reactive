
using oxy = OxyPlot;
using System;
using System.Collections.Generic;
using System.Text;
using ReactivePlot.Model;

namespace ReactivePlot.OxyPlot.Common
{

    public interface IOxyPlotModel<T> : IPlotModel<T>, IOxyPlotModel
    {

    }

    public interface IOxyPlotModel : IPlotModel, IPlotModelWrap
    {

    }

    public interface IPlotModelWrap
    {
        public oxy.PlotModel PlotModel { get; }
    }

}

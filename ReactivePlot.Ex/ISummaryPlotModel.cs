using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReactivePlot.Ex
{
    public interface ISummaryPlotModel : IPlotModel
    {
        public SummaryListBox PlotModel { get; }
    }

    public interface ISummaryPlotModel<T> : IPlotModel<T>
    {
        public SummaryListBox PlotModel { get; }
    }
}

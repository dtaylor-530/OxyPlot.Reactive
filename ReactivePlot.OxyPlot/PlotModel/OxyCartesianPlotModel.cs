using MoreLinq;
using OxyPlot;
using OxyPlot.Series;
using ReactivePlot.Model;
using ReactivePlot.OxyPlot.Common;
using System;
using System.Linq;
using System.Reactive.Linq;
using oxy = OxyPlot;
namespace ReactivePlot.OxyPlot.PlotModel
{

    public class OxyCartesianPlotModel<TKey> : OxyCartesianPlotModel<TKey, IDoublePoint<TKey>>
    {
        public OxyCartesianPlotModel(oxy.PlotModel plotModel) : base(plotModel)
        {
        }
    }


    public class OxyCartesianPlotModel<TKey, TPoint> : OxyPlotModel<TPoint> where TPoint : IDoublePoint<TKey>
    {
        public OxyCartesianPlotModel(oxy.PlotModel plotModel) : base(plotModel)
        {
        }

        protected override TPoint OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TPoint[] items)
        {
            var x = series.InverseTransform(e.Position).X;
            var point = items.MinBy(a => Math.Abs(a.Var - x)).First();
            return point;
        }

        protected override IDataPointProvider Convert(TPoint item)
        {
            return new DataPointProvider(item.Var, item.Value);
        }
    }
}

using MoreLinq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactivePlot.Model;
using ReactivePlot.OxyPlot.Common;
using System;
using System.Linq;
using oxy = OxyPlot;
namespace ReactivePlot.OxyPlot.PlotModel
{

    public class OxyTimePlotModel<TKey> : OxyTimePlotModel<TKey, ITimePoint<TKey>>
    {
        public OxyTimePlotModel(oxy.PlotModel plotModel) : base(plotModel)
        {
        }
    }

    public class OxyTimePlotModel<TKey, T> : OxyPlotModel<T> where T: ITimePoint<TKey>
    {
        public OxyTimePlotModel(oxy.PlotModel plotModel) : base(plotModel)
        {
        }

        protected override T OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, T[] items)
        {
            var time = DateTimeAxis.ToDateTime(series.InverseTransform(e.Position).X);
            var point = items.MinBy(a => Math.Abs((a.Var - time).Ticks)).First();
            return point;
        }

        protected override void Configure()
        {
            PlotModel.Axes.Add(new DateTimeAxis());

        }

        protected override IDataPointProvider Convert(T item)
        {
            return new DataPointProvider(item.Var, item.Value);
        }
    }
}

#nullable enable

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactivePlot.Model;
using ReactivePlot.OxyPlot.Common;
using ReactivePlot.OxyPlot.PlotModel;
using System.Linq;
using System.Reactive.Linq;
using OxyPlotModel = OxyPlot.PlotModel;

namespace ReactivePlot.OxyPlot

{ 
    public class ErrorBarPlotModel : IOxyPlotModel<(string key, ErrorPoint)>
    {
        private static readonly OxyColor Positive = OxyColor.Parse("#0074D9");
        private static readonly OxyColor Negative = OxyColor.Parse("#FF4136");

        public global::OxyPlot.PlotModel PlotModel { get; }

        public ErrorBarPlotModel(OxyPlotModel plotModel)
        {
          
            this.PlotModel = plotModel;
            Configure();
        }

        protected virtual void Configure()
        {
            var linearAxis1 = new LinearAxis
            {
                //linearAxis1.AbsoluteMinimum = 0;
                Key = "x"
            };
            //linearAxis1.MaximumPadding = 0.06;
            //linearAxis1.MinimumPadding = 0;
            lock (PlotModel)
                PlotModel.Axes.Add(linearAxis1);

        }

        public bool RemoveSeries(string title)
        {
            lock (PlotModel)
            {
                if (PlotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series)
                {
                    //disposableDictionary.Remove(title);
                    PlotModel.Series.Remove(series);
                    return true;
                }
                return false;
            }
        }

        public virtual void AddToSeries((string key, ErrorPoint)[] points, string title, int? index)
        {
            PlotModel.Series.Add(OxyFactory.BuildError(
                points.Select(a => a.Item2).Select(a => new ErrorColumnItem(a.Value, a.Deviation) { Color = a.Value > 0 ? Positive : Negative }).ToArray(), title));

            for (int i = PlotModel.Axes.Count - 1; i > -1; i--)
            {
                if (PlotModel.Axes[i].Key == "y")
                    PlotModel.Axes.RemoveAt(i);
            }

            var categoryAxis1 = new CategoryAxis
            {
                Key = "y",
                MinorStep = 1
            };

            foreach (var key in points.Select(p => p.key))
            {
                categoryAxis1.Labels.Add(key);
                //categoryAxis1.ActualLabels.Add(key);
            }
            PlotModel.Axes.Add(categoryAxis1);
        }

        public void ClearSeries()
        {
            PlotModel.Series.Clear();
        }

        public void Invalidate(bool v)
        {
            PlotModel.InvalidatePlot(v);
        }
    }
}
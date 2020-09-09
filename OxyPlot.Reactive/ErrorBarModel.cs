#nullable enable

using MathNet.Numerics.Statistics;
using OxyPlot.Axes;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    public class ErrorBarModel : SinglePlotModel<string>
    {
        private static readonly OxyColor Positive = OxyColor.Parse("#0074D9");
        private static readonly OxyColor Negative = OxyColor.Parse("#FF4136");

        public ErrorBarModel(PlotModel plotModel, SynchronizationContext? context = null, IScheduler? scheduler = null) : base(plotModel, context: context, scheduler: scheduler)
        {
        }

        protected override async void Refresh(IList<Unit> units)
        {
            var points = await Task.Run(() =>
            {
                lock (lck)
                {
                    return DataPoints.GroupBy(a => a.X).ToArray().Select(Selector).OrderBy(a => a.Item2.Value).ToArray();
                }
            });

            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (plotModel)
                {
                    plotModel.Series.Clear();
                    AddToSeries(points, "A Title");
                    plotModel.InvalidatePlot(true);
                }
            });
        }

        //static (string key, ErrorBarItem) Selector(IGrouping<string, DataPoint<string>> grp)
        private static (string key, ErrorColumnItem) Selector(IGrouping<string, XY<string>> grp)
        {
            var arr = grp.Select(a => a.Y).ToArray();
            // var variance = Statistics.Variance(arr);
            var sd = arr.StandardDeviation();
            var mean = arr.Mean();
            // return (grp.Key, new ErrorBarItem(mean, sd) { Color = mean > 0 ? Positive : Negative });
            return (grp.Key, new ErrorColumnItem(mean, sd) { Color = mean > 0 ? Positive : Negative });
        }

        protected override void ModifyPlotModel()
        {
            var linearAxis1 = new LinearAxis
            {
                //linearAxis1.AbsoluteMinimum = 0;
                Key = "x"
            };
            //linearAxis1.MaximumPadding = 0.06;
            //linearAxis1.MinimumPadding = 0;
            lock (lck)
                plotModel.Axes.Add(linearAxis1);

            base.ModifyPlotModel();
        }

        //protected virtual void AddToSeries((string key, ErrorBarItem)[] points, string title)
        protected virtual void AddToSeries((string key, ErrorColumnItem)[] points, string title)
        {
            plotModel.Series.Add(OxyFactory.Build(points.Select(a => a.Item2).ToArray(), title));

            for (int i = plotModel.Axes.Count - 1; i > -1; i--)
            {
                if (plotModel.Axes[i].Key == "y")
                    plotModel.Axes.RemoveAt(i);
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
            plotModel.Axes.Add(categoryAxis1);
        }
    }
}
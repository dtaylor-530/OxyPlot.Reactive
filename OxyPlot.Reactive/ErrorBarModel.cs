﻿using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;

namespace OxyPlot.Reactive
{
    public class ErrorBarModel : SinglePlotModel<string>
    {
        static readonly OxyColor Positive = OxyColor.Parse("#0074D9");
        static readonly OxyColor Negative = OxyColor.Parse("#FF4136");

        public ErrorBarModel(IDispatcher dispatcher, PlotModel plotModel) : base(dispatcher, plotModel)
        {
        }

        protected override void Refresh(IList<Unit> units)
        {
            dispatcher.BeginInvoke(async () =>
            {
                plotModel.Series.Clear();
                _ = await Task.Run(() =>
                {
                    lock (lck)
                    {
                        return DataPoints.GroupBy(a => a.X).ToArray().Select(Selector).OrderBy(a => a.Item2.Value).ToArray();
                    }

                }).ContinueWith(async points =>
                {
                    AddToSeries(await points, "A Title");
                }, TaskScheduler.FromCurrentSynchronizationContext());


                plotModel.InvalidatePlot(true);
            });

            static (string key, ErrorColumnItem) Selector(IGrouping<string, DataPoint<string>> grp)
            {
                var arr = grp.Select(a => a.Y).ToArray();
                // var variance = Statistics.Variance(arr);
                var sd = arr.StandardDeviation();
                var mean = arr.Mean();
                return (grp.Key, new ErrorColumnItem(mean, sd) { Color = mean > 0 ? Positive : Negative });
            }
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
            plotModel.Axes.Add(linearAxis1);

            base.ModifyPlotModel();
        }


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




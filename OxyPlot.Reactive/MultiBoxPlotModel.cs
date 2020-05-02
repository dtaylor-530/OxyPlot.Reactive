using OxyPlot;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    public class MultiBoxPlotModel : MultiPlotModel<string, int>
    {
        public MultiBoxPlotModel(IDispatcher dispatcher, PlotModel plotModel) : base(dispatcher, plotModel)
        {
        }

        public MultiBoxPlotModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<string> comparer) : base(dispatcher, model, comparer)
        {
        }


        protected override void Refresh(IList<Unit> units)
        {

            dispatcher.BeginInvoke(async () =>
            {
                plotModel.Series.Clear();
                foreach (var keyValue in DataPoints.ToArray())
                {
                    _ = await Task.Run(() =>
                      {
                          lock (lck)
                          {
                              return SelectBPI(keyValue.Value);
                          }

                      }).ContinueWith(async points =>
                      {
                          AddToSeries(await points, keyValue.Key.ToString());
                      }, TaskScheduler.FromCurrentSynchronizationContext());
                }


                if (showAll)
                {
                    _ = await Task.Run(() =>
                      {
                          lock (lck)
                          {
                              return SelectBPI(DataPoints.SelectMany(a => a.Value));
                          }

                      }).ContinueWith(async points =>
                      {
                          AddToSeries(await points, "All");
                      }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                plotModel.InvalidatePlot(true);
            });

            static BoxPlotItem[] SelectBPI(IEnumerable<(int X, double Y)> dataPoints)
            {
                return dataPoints.GroupBy(a => a.X)
                              .Select(Selector)
                              .ToArray();
            }

            static BoxPlotItem Selector(IGrouping<int, (int X, double Y)> grp)
            {
                var arr = grp.Select(a => a.Y).ToArray();
                var variance = MathNet.Numerics.Statistics.Statistics.Variance(arr);
                var sd = MathNet.Numerics.Statistics.Statistics.StandardDeviation(arr);
                var median = MathNet.Numerics.Statistics.Statistics.Mean(arr);
                return new BoxPlotItem(grp.Key, median - variance, median - sd, median, median + sd, median + variance)
                { Mean = median, Tag = "A Tag" };

            };
        }


        protected virtual void AddToSeries(BoxPlotItem[] points, string title)
        {
            plotModel.Series.Add(OxyFactory.Build(points, title));
        }

    }
}




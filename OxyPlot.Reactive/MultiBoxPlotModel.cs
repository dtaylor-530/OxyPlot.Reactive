using OxyPlot;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    public class MultiBoxPlotModel : MultiPlotModel<string, int>
    {
        public MultiBoxPlotModel(PlotModel plotModel, IScheduler? scheduler = null) : base(plotModel, scheduler: scheduler)
        {
        }

        public MultiBoxPlotModel(PlotModel model, IEqualityComparer<string> comparer, IScheduler scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }


        protected override void Refresh(IList<Unit> units)
        {
            
            scheduler.Schedule(async () =>
           {
               lock (lck)
                   plotModel.Series.Clear();


               foreach (var keyValue in DataPoints.ToArray())
               {
                    await Task.Run(() =>
                     {
                         lock (lck)
                         {
                             return SelectBPI(keyValue.Value);
                         }

                     }).ContinueWith(points =>
                     {
                         scheduler.Schedule(async()=>

                       AddToSeries(await points, keyValue.Key.ToString()));
                     });
               }


               if (showAll)
               {
                   await Task.Run(() =>
                     {
                         return SelectBPI(DataPoints.SelectMany(a => a.Value));
                     }).ContinueWith(points =>
                     {
                         scheduler.Schedule(async()=>
 
                          AddToSeries(await points, "All"));
                     });
               }

               lock (lck)
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
            scheduler.Schedule(() =>
            {
                lock (lck)
                    plotModel.Series.Add(OxyFactory.Build(points, title));
            });
        }

    }
}




# nullable enable
using OxyPlot.Reactive.Infrastructure;
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
    public class MultiBoxPlotModel : MultiPlotModel<string, int>
    {


        public MultiBoxPlotModel(PlotModel plotModel) : base(plotModel, context: SynchronizationContext.Current)
        {
        }

        public MultiBoxPlotModel(PlotModel model, IEqualityComparer<string> comparer) : base(model, comparer, context: SynchronizationContext.Current)
        {

        }
        public MultiBoxPlotModel(PlotModel plotModel, IScheduler scheduler) : base(plotModel, scheduler: scheduler)
        {
        }

        public MultiBoxPlotModel(PlotModel model, IEqualityComparer<string> comparer, IScheduler scheduler) : base(model, comparer, scheduler: scheduler)
        {
        }

        public MultiBoxPlotModel(PlotModel plotModel, SynchronizationContext context) : base(plotModel, context: context)
        {
        }

        public MultiBoxPlotModel(PlotModel model, IEqualityComparer<string> comparer, SynchronizationContext context) : base(model, comparer, context: context)
        {
        }


        protected override async void Refresh(IList<Unit> units)
        {
            KeyValuePair<string, List<(int x, double y)>>[] arr;

            lock (lck)
            {
                arr = DataPoints.ToArray();
            }

            List<(string, BoxPlotItem[])> list = new List<(string, BoxPlotItem[])>();

            foreach (var keyValue in arr)
            {
                list.Add((keyValue.Key.ToString(), await Task.Run(() =>
                 {
                     var ar = keyValue.Value.ToArray();
                     return SelectBPI(ar);
                 })));
            }


            if (showAll)
            {
                list.Add(("All", await Task.Run(() => SelectBPI(arr.SelectMany(a => a.Value).ToArray()))));
            }


            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (plotModel)
                {
                    plotModel.Series.Clear();

                    foreach (var item in list)
                        AddToSeries(item.Item2, item.Item1);

                    plotModel.InvalidatePlot(true);
                }
            });


            static BoxPlotItem[] SelectBPI(IList<(int X, double Y)> dataPoints)
            {
                return dataPoints.GroupBy(a => a.X)
                          .Select(Selector)
                          .ToArray();


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

        }

        protected virtual void AddToSeries(BoxPlotItem[] points, string title)
        {
            plotModel.Series.Add(OxyFactory.Build(points, title));
        }

    }
}




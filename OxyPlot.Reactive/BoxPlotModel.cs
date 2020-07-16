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
    public class BoxPlotModel : MultiPlotModel<string, int>
    {


        public BoxPlotModel(PlotModel plotModel) : base(plotModel, context: SynchronizationContext.Current)
        {
        }

        public BoxPlotModel(PlotModel model, IEqualityComparer<string> comparer) : base(model, comparer, context: SynchronizationContext.Current)
        {

        }
        public BoxPlotModel(PlotModel plotModel, IScheduler scheduler) : base(plotModel, scheduler: scheduler)
        {
        }

        public BoxPlotModel(PlotModel model, IEqualityComparer<string> comparer, IScheduler scheduler) : base(model, comparer, scheduler: scheduler)
        {
        }

        public BoxPlotModel(PlotModel plotModel, SynchronizationContext context) : base(plotModel, context: context)
        {
        }

        public BoxPlotModel(PlotModel model, IEqualityComparer<string> comparer, SynchronizationContext context) : base(model, comparer, context: context)
        {
        }


        protected override async void Refresh(IList<Unit> units)
        {
            KeyValuePair<string, ICollection<KeyValuePair<int, double>>>[] arr;

            lock (DataPoints)
            {
                arr = DataPoints.ToArray();
            }

            (this as IMixedScheduler).ScheduleAction(async () =>
            {
                var botPlotItems = await Task.Run(() => Transform(arr, showAll));

                lock (plotModel)
                {
                    plotModel.Series.Clear();

                    foreach (var item in botPlotItems)
                        AddToSeries(item.Item2, item.Item1);

                    plotModel.InvalidatePlot(true);
                }
            });


            static BoxPlotItem[] SelectBPI(IList<KeyValuePair<int, double>> dataPoints)
            {
                return dataPoints.GroupBy(a => a.Key)
                          .Select(Selector)
                          .ToArray();


                static BoxPlotItem Selector(IGrouping<int, KeyValuePair<int, double>> grp)
                {
                    var arr = grp.Select(a => a.Value).ToArray();
                    var variance = MathNet.Numerics.Statistics.Statistics.Variance(arr);
                    var sd = MathNet.Numerics.Statistics.Statistics.StandardDeviation(arr);
                    var median = MathNet.Numerics.Statistics.Statistics.Mean(arr);
                    return new BoxPlotItem(grp.Key, median - variance, median - sd, median, median + sd, median + variance)
                    { Mean = median, Tag = "A Tag" };

                };
            }

            static (string, BoxPlotItem[])[] Transform(KeyValuePair<string, ICollection<KeyValuePair<int, double>>>[] arr, bool showAll)
            {
                (string, BoxPlotItem[])[] combinedArray = new (string, BoxPlotItem[])[(arr.Length + System.Convert.ToInt32(showAll))];
                ICollection<KeyValuePair<int, double>>[] valuePairs = new ICollection<KeyValuePair<int, double>>[arr.Length];

                int i = 0;
                foreach (var keyValue in arr)
                {
                    var arrA = keyValue.Value.ToArray();
                    valuePairs[i] = arrA;
                    combinedArray[i++] = (keyValue.Key.ToString(), SelectBPI(arrA));
                }

                if (showAll)
                {
                    var arrAll = valuePairs.SelectMany(a => a).ToArray();
                    combinedArray[i] = ("All", SelectBPI(arrAll));
                }

                return combinedArray;
            }

        }

        protected virtual void AddToSeries(BoxPlotItem[] points, string title)
        {
            plotModel.Series.Add(OxyFactory.Build(points, title));
        }

    }
}




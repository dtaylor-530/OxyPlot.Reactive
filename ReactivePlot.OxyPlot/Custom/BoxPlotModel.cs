﻿#nullable enable

using OxyPlot.Series;
using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.Model;
using ReactivePlot.OxyPlot.Common;
using ReactivePlot.OxyPlot.PlotModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OxyPlotModel = OxyPlot.PlotModel;
using stats = MathNet.Numerics.Statistics.Statistics;

namespace ReactivePlot.OxyPlot
{
    public class BoxPlotModel : SeriesBaseModel<string, int>, IObserver<bool>
    {
        IMultiPlotModel<BoxPlotItem> plotModel;
        private bool showAll;

        public BoxPlotModel(OxyPlotModel plotModel, IEqualityComparer<string>? comparer = default, IScheduler? scheduler = default) : this(new BoxPlotPlotModel(plotModel), comparer, scheduler)
        {
        }

        public BoxPlotModel(OxyPlotModel plotModel, IEqualityComparer<string> comparer = default, SynchronizationContext? context = default) : this(new BoxPlotPlotModel(plotModel), comparer, context)
        {
        }

        public BoxPlotModel(IMultiPlotModel<BoxPlotItem> plotModel) : base(plotModel, context: SynchronizationContext.Current)
        {
            this.plotModel = plotModel;
        }

        public BoxPlotModel(IMultiPlotModel<BoxPlotItem> plotModel, IEqualityComparer<string> comparer) : base(plotModel, comparer, context: SynchronizationContext.Current)
        {
            this.plotModel = plotModel;
        }

        public BoxPlotModel(IMultiPlotModel<BoxPlotItem> plotModel, IScheduler scheduler) : base(plotModel, scheduler: scheduler)
        {
            this.plotModel = plotModel;
        }

        public BoxPlotModel(IMultiPlotModel<BoxPlotItem> plotModel, IEqualityComparer<string> comparer, IScheduler scheduler) : base(plotModel, comparer, scheduler: scheduler)
        {
            this.plotModel = plotModel;
        }

        public BoxPlotModel(IMultiPlotModel<BoxPlotItem> plotModel, SynchronizationContext context) : base(plotModel, context: context)
        {
            this.plotModel = plotModel;
        }

        public BoxPlotModel(IMultiPlotModel<BoxPlotItem> plotModel, IEqualityComparer<string> comparer, SynchronizationContext context) : base(plotModel, comparer, context: context)
        {
            this.plotModel = plotModel;
        }

        protected override void Refresh(IList<Unit> units)
        {
            KeyValuePair<string, ICollection<KeyValuePair<int, double>>>[] arr;

            (this as IMixedScheduler).ScheduleAction(async () =>
            {
                var botPlotItems = await Task.Run(() =>
                {
                    lock (DataPoints)
                        return Transform(DataPoints.ToArray(), showAll);
                });

                lock (plotModel)
                {
                    plotModel.Clear();

                    foreach (var item in botPlotItems)
                        plotModel.AddSeries(item.Item2, item.Item1);

                    plotModel.Invalidate(true);
                }
            });



            static (string, BoxPlotItem[])[] Transform(KeyValuePair<string, ICollection<KeyValuePair<int, double>>>[] arr, bool showAll)
            {
                (string, BoxPlotItem[])[] combinedArray = new (string, BoxPlotItem[])[(arr.Length + Convert.ToInt32(showAll))];
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

                static BoxPlotItem[] SelectBPI(IList<KeyValuePair<int, double>> dataPoints)
                {
                    return dataPoints.GroupBy(a => a.Key)
                               .Select(Selector)
                               .ToArray();

                    static BoxPlotItem Selector(IGrouping<int, KeyValuePair<int, double>> grp)
                    {
                        var arr = grp.Select(a => a.Value).ToArray();
                        var variance = stats.Variance(arr);
                        var sd = stats.StandardDeviation(arr);
                        var median = stats.Mean(arr);
                        return new BoxPlotItem(grp.Key, median - variance, median - sd, median, median + sd, median + variance)
                        { Mean = median, Tag = "A Tag" };
                    };
                }
            }
        }

        public void OnNext(bool value)
        {
            showAll = true;
            refreshSubject.OnNext(Unit.Default);
        }
    }


    public class BoxPlotPlotModel : OxyBasePlotModel, IMultiPlotModel<BoxPlotItem>
    {
        public BoxPlotPlotModel(OxyPlotModel plotModel) : base(plotModel)
        {
        }

        public void AddSeries(IReadOnlyCollection<BoxPlotItem> items, string title, int? index = null)
        {
            PlotModel.Series.Add(OxyFactory.BuildBoxPlot(items, title));
        }
    }
}
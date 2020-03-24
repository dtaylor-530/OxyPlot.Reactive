
namespace OxyPlotEx.ViewModel
{
    using MoreLinq;
    using OxyPlot;
    using OxyPlot.Axes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using e = System.Linq.Enumerable;

    public class MultiLineModel<T> : MultiPlotModel<T, DateTime>
    {

        public MultiLineModel(IDispatcher dispatcher, PlotModel plotModel) : base(dispatcher, plotModel)
        {
        }

        public MultiLineModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : base(dispatcher, model,comparer)
        {
        }

        protected override void ModifyPlotModel()
        {
            plotModel.Axes.Add(new DateTimeAxis());
        }

        protected override void Refresh(IList<Unit> units)
        {
            this.dispatcher.BeginInvoke(async () =>
            {
                plotModel.Series.Clear();
                foreach (var keyValue in DataPoints.ToArray())
                {
                    await Task.Run(() =>
                    {
                        lock (lck)
                        {

                            var points = keyValue.Value
                                            .OrderBy(c => c.X)
                                            .Scan((xy0, xy) => new DataPoint<DateTime>(xy.X, Combine(xy0.Y, xy.Y))).ToArray();
                            return points;
                        }

                    }).ContinueWith(async points =>
                    {
                        AddToSeries(await points, keyValue.Key.ToString());
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }

                if (ShowAll)
                {
                    await Task.Run(() =>
                    {
                        lock (lck)
                        {
                            var allPoints = DataPoints
                            .SelectMany(a => a.Value)
                            .OrderBy(c => c.X)
                            .Scan((xy0, xy) => new DataPoint<DateTime>(xy.X, Combine(xy0.Y, xy.Y)));
                            return allPoints.ToArray();
                        }

                    }).ContinueWith(async points =>
                    {
                        AddToSeries(await points, "All");
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                plotModel.InvalidatePlot(true);
            });
        }

        protected virtual void AddToSeries(DataPoint<DateTime>[] points, string title)
        {
            plotModel.Series.Add(OxyFactory.Build(points.Select(a=>new DateTimePoint(a.X,a.Y)), title));
        }
    }
}



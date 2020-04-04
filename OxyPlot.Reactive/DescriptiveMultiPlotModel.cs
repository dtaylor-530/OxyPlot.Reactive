#nullable enable
using MoreLinq;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using e = System.Linq.Enumerable;
//https://www.codeproject.com/Tips/1075272/Multi-line-Tracker-for-OxyPlot

namespace OxyPlotEx.ViewModel
{
    public class DescriptiveMultiPlotModel : MultiPlotModelBase<string,IDataPointProvider>
    {


        public DescriptiveMultiPlotModel(IDispatcher dispatcher, PlotModel plotModel) : base(dispatcher, plotModel)
        {
        }

        public DescriptiveMultiPlotModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<string> comparer) : base(dispatcher, model, comparer)
        {
        }

        protected override void ModifyPlotModel()
        {
            //plotModel.Axes.Add(new DateTimeAxis());
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

                            var points = keyValue.Value.Select(a=>a.GetDataPoint())
                                            .OrderBy(c => c.X)
                                            .Scan((xy0, xy) => new DataPoint(xy.X, Combine(xy0.Y, xy.Y))).ToArray();
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
                            .SelectMany(a => a.Value.Select(a => a.GetDataPoint())
                            .OrderBy(c => c.X)
                            .Scan((xy0, xy) => new DataPoint(xy.X, Combine(xy0.Y, xy.Y))));
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

        protected virtual void AddToSeries(DataPoint[] points, string title)
        {
            plotModel.Series.Add(OxyFactory.Build(points, title));
        }
    }



    public class WpbTrackerManipulator2 : MouseManipulator
    {
        /// <summary>
        /// The current series.
        /// </summary>
        private XYAxisSeries? currentSeries;

        public WpbTrackerManipulator2(IPlotView plotView)
            : base(plotView)
        {
        }

        /// <summary>
        /// Occurs when a manipulation is complete.
        /// </summary>
        /// <param name="e">
        /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
        /// </param>
        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);
            e.Handled = true;

            currentSeries = null;
            PlotView.HideTracker();
        }

        /// <summary>
        /// Occurs when the input device changes position during a manipulation.
        /// </summary>
        /// <param name="e">
        /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
        /// </param>
        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            e.Handled = true;

            if (currentSeries == null
                || PlotView.ActualModel == null
                || PlotView.ActualModel.PlotArea.Contains(e.Position.X, e.Position.Y) == false)
            {
                PlotView.HideTracker();
                return;
            }

            var points =  GetDataPoints().ToArray();
            var result = new WpbTrackerHitResult(points)
            {
                Series = currentSeries,
                DataPoint = currentSeries.ItemsSource.Cast<DataPoint>().First(),
                Index = e.Position.X,
                Item =  currentSeries.ItemsSource.Cast<DataPoint>().First(),
                Position = e.Position,
                PlotModel = PlotView.ActualModel
            };
            PlotView.ShowTracker(result);

            IEnumerable<double> GetDataPoints()
            {
                foreach (XYAxisSeries currentSeries in PlotView.ActualModel.Series.OfType<XYAxisSeries>())
                {
                    var ps = currentSeries.ItemsSource.Cast<DataPoint>();
                    var time = currentSeries.InverseTransform(e.Position).X;
                    var dp = ps?.FirstOrDefault(d => d.X >= time);

                    if (dp.HasValue && (dp.Value.X != 0 || dp.Value.Y != 0))
                    {
                        var position = new ScreenPoint(XAxis.Transform(dp.Value.X, dp.Value.Y, currentSeries.YAxis).X, e.Position.Y);
                        var inddex = ps.Select((d, i) => (dp = d, i)).Where(a => a.Item1.HasValue).FirstOrDefault(a => a.Item1.Value.Equals(dp.Value)).Item1;

                        if(inddex.HasValue)
                        yield return inddex.Value.Y;
                    }
                }
            }
         }


        /// <summary>
        /// Occurs when an input device begins a manipulation on the plot.
        /// </summary>
        /// <param name="e">
        /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
        /// </param>
        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);
            currentSeries = PlotView.ActualModel.Series.OfType<XYAxisSeries>()
                             .FirstOrDefault(s => s.IsVisible);
            Delta(e);
        }
    }

    //        Notice that I'm also using a new WpbTrackerHitResult class to package the values from all the series:
    //Hide Copy Code

    public class WpbTrackerHitResult2 : TrackerHitResult
    {
        public double[] Values { get; private set; }

        // can't use the default indexer name (Item) since the base class uses that for something else
        [System.Runtime.CompilerServices.IndexerName("ValueString")]
        public string this[int index]
        {
            get
            {
                return string.Format((index == 1 || index == 4) ?
                  "{0,7:###0   }" : "{0,7:###0.0#}", Values[index]);
            }
        }

        public WpbTrackerHitResult2(double[] values)
        {
            Values = values;
        }
    }

}




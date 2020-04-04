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
    public class DescriptivePlotModel : IObserver<IDataPointProvider>, IObserver<string>
    {

        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly IDispatcher dispatcher;
        protected readonly PlotModel plotModel;
        protected readonly object lck = new object();
        protected List<IDataPointProvider> DataPoints = new List<IDataPointProvider>();
        private string formatString = "X={2},\nY={4},\nAdditionalInfo={Description}";

        public DescriptivePlotModel(IDispatcher dispatcher, PlotModel plotModel)
        {
            this.dispatcher = dispatcher;
            this.plotModel = plotModel;
            ModifyPlotModel();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(100)).Where(e.Any).Subscribe(Refresh);
        }

        protected virtual void ModifyPlotModel()
        {

        }

        public void OnNext(IDataPointProvider item)
        {
            Task.Run(() => AddToDataPoints(item)).ToObservable().Subscribe(refreshSubject.OnNext);
        }

        protected virtual double Combine(double x0, double x1) => x1;

        public void Reset() => Task.Run(() => RemoveByPredicate(a => true)).ToObservable().Subscribe(refreshSubject.OnNext);

        //public void Remove(ISet<T> names) => Task.Run(() => RemoveByPredicate(s => names.Contains(s.))).ToObservable().Subscribe(refreshSubject.OnNext);

        public void OnCompleted() { }

        public void OnError(Exception error) => throw new NotImplementedException($"Error in {nameof(DescriptivePlotModel)}");

        private void AddToDataPoints(IDataPointProvider item)
        {
            lock (lck)
            {
                DataPoints.Add(item);
            }
        }

        protected void Refresh(IList<Unit>? units)
        {

            this.dispatcher.BeginInvoke(async () =>
            {
                plotModel.Series.Clear();
                _ = await Task.Run(() =>
                {
                    lock (lck)
                    {
                        return DataPoints.ToArray();
                    }

                }).ContinueWith(async points =>
                {
                    AddToSeries(await points, formatString, "A Title");
                }, TaskScheduler.FromCurrentSynchronizationContext());


                plotModel.InvalidatePlot(true);
            });
        }

        private void AddToSeries(IDataPointProvider[] points, string format, string title)
        {
            plotModel.Series.Add(OxyFactory.Build(points, format, title));
        }

        private void RemoveByPredicate(Predicate<IDataPointProvider> predicate)
        {

            lock (lck)
            {
                foreach (var dataPoint in DataPoints.Where(a => predicate(a)))
                    DataPoints.Remove(dataPoint);
            }
        }

        public void OnNext(string value)
        {
            lock (lck)
            {
                formatString = value;
            }

            Refresh(null);
        }
    }



    public class WpbTrackerManipulator : MouseManipulator
    {
        /// <summary>
        /// The current series.
        /// </summary>
        private XYAxisSeries? currentSeries;

        public WpbTrackerManipulator(IPlotView plotView)
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
        public override async void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            e.Handled = true;

            if (currentSeries == null || PlotView.ActualModel == null || PlotView.ActualModel.PlotArea.Contains(e.Position.X, e.Position.Y) == false)
                PlotView.HideTracker();
            else if (GetResult(PlotView.ActualModel, currentSeries, e, XAxis) is WpbTrackerHitResult wpb && wpb != null)
                PlotView.ShowTracker(wpb);
            else
                PlotView.HideTracker();
        }


        static WpbTrackerHitResult? GetResult(PlotModel plotModel, XYAxisSeries currentSeries, OxyMouseEventArgs e, OxyPlot.Axes.Axis XAxis)
        {

            var ps = currentSeries.ItemsSource.Cast<IDataPointProvider>().ToArray();

            // Exclude default DataPoint.
            // It has insignificant downside and is more performant than using First above
            // and handling exceptions.
            var time = currentSeries.InverseTransform(e.Position).X;
            var dp = ps?.FirstOrDefault(d => d.GetDataPoint().X >= time)?.GetDataPoint();
            if (dp.HasValue && (dp.Value.X != 0 || dp.Value.Y != 0))
            {
                var position = new ScreenPoint(XAxis.Transform(dp.Value.X, dp.Value.Y, currentSeries.YAxis).X, e.Position.Y);
                int index = ps.Select((d, i) => (dpoint : d.GetDataPoint(), i)).FirstOrDefault(a =>
                a.dpoint.Equals(dp.Value)).i;
                if (index != 0)
                    return new WpbTrackerHitResult(GetDataPoints(index, plotModel).ToArray())
                    {
                        Series = currentSeries,
                        DataPoint = dp.Value,
                        Index = index,
                        Item = dp.Value,
                        Position = position,
                        PlotModel = plotModel
                    };

            }
            return null;

            static IEnumerable<double> GetDataPoints(int index, PlotModel plotModel)
            {
                foreach (XYAxisSeries series in plotModel.Series.OfType<XYAxisSeries>())
                {
                    yield return (series.ItemsSource.Cast<IDataPointProvider>()).Skip(index - 1).First().GetDataPoint().Y;
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

    public class WpbTrackerHitResult : TrackerHitResult
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

        public WpbTrackerHitResult(double[] values)
        {
            Values = values;
        }
    }

}




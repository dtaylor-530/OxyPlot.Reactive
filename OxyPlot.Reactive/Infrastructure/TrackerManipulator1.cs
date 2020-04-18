#nullable enable

using OxyPlot;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OxyPlot.Series;
using MoreLinq;
using System.Drawing;

namespace OxyPlotEx.ViewModel
{
    /// <summary>
    /// Provides a plot manipulator for tracker functionality.
    /// </summary>
    public class TrackerManipulator1 : TrackerManipulator
    {
        /// <summary>
        /// The current series.
        /// </summary>
        private Series? currentSeries;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerManipulator" /> class.
        /// </summary>
        /// <param name="plotView">The plot view.</param>
        public TrackerManipulator1(IPlotView plotView)
            : base(plotView)
        {
        }

        /// <summary>
        /// Occurs when the input device changes position during a manipulation.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            e.Handled = true;
            if (Return()) return;

            var results = SelectResults().ToArray();
            var first = results.FirstOrDefault();

            if (first != null)
            {
                var htr = new TrackerHitResult1(results.Where(a => a != null).ToDictionary(a => a.Text.Split('\n').First(), a =>
                 {
                     var c = this.PlotView.ActualModel.Series.OfType<LineSeries>().SingleOrDefault(s => s.Title == a.Text.Split('\n').First()).Color;
                     return new TrackerHitResult1.ValueAndBrush
                     {
                         Color = Color.FromArgb(c.A, c.R, c.G, c.B),
                         Value = a.DataPoint.Y,
                     };
                 }), first);
                this.PlotView.ShowTracker(htr);
                this.PlotView.ActualModel.RaiseTrackerChanged(htr);
            }

            IEnumerable<TrackerHitResult?> SelectResults()
            =>
                from series in PlotView.ActualModel.Series.OfType<XYAxisSeries>()
                orderby series.Title == currentSeries?.Title descending
                select GetNearestHit(series, e.Position, this.Snap, this.PointsOnly);


            bool Return()
            {
                var actualModel = this.PlotView.ActualModel;
                if (actualModel == null || !actualModel.PlotArea.Contains(e.Position.X, e.Position.Y))
                {
                    return true;
                }

                if (this.currentSeries == null || !this.LockToInitialSeries)
                {
                    // get the nearest
                    this.currentSeries = actualModel.GetSeriesFromPoint(e.Position, 20);

                    if (this.currentSeries == null)
                    {
                        if (!this.LockToInitialSeries)
                        {
                            this.PlotView.HideTracker();
                        }

                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the nearest tracker hit.
        /// </summary>
        /// <param name="series">The series.</param>
        /// <param name="point">The point.</param>
        /// <param name="snap">Snap to points.</param>
        /// <param name="pointsOnly">Check points only (no interpolation).</param>
        /// <returns>A tracker hit result.</returns>
        private static TrackerHitResult? GetNearestHit(Series series, ScreenPoint point, bool snap, bool pointsOnly)
        {
            // Check data points only
            if (snap || pointsOnly)
            {
                var result = series?.GetNearestPoint(point, false);
                if (result != null && result.Position.DistanceTo(point) < 20)
                {
                    return result;
                }
            }

            // Check between data points (if possible)
            if (!pointsOnly)
            {
                return series?.GetNearestPoint(point, true);
            }

            return null;
        }
    }
}

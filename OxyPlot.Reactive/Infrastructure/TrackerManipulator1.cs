#nullable enable

using MoreLinq;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;

namespace OxyPlot.Reactive.Infrastructure
{
    /// <summary>
    /// Provides a plot manipulator for tracker functionality.
    /// </summary>
    public class TrackerManipulator1 : TrackerManipulator
    {
        /// <summary>
        /// The current series.
        /// </summary>
        private Series.Series? currentSeries;

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
        /// <param name="e">The <see cref="OxyMouseEventArgs" /> instance containing the event data.</param>
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
                    var c = PlotView.ActualModel.Series.OfType<LineSeries>().SingleOrDefault(s => s.Title == a.Text.Split('\n').First()).Color;
                    return new TrackerHitResult1.ValueAndBrush
                    {
                        Color = Color.FromArgb(c.A, c.R, c.G, c.B),
                        Value = a.DataPoint.Y,
                    };
                }), first);
                PlotView.ShowTracker(htr);
                PlotView.ActualModel.RaiseTrackerChanged(htr);
            }

            IEnumerable<TrackerHitResult?> SelectResults()
            =>
                from series in PlotView.ActualModel.Series.OfType<XYAxisSeries>()
                orderby series.Title == currentSeries?.Title descending
                select GetNearestHit(series, e.Position, Snap, PointsOnly);

            bool Return()
            {
                var actualModel = PlotView.ActualModel;
                if (actualModel == null || !actualModel.PlotArea.Contains(e.Position.X, e.Position.Y))
                {
                    return true;
                }

                if (currentSeries == null || !LockToInitialSeries)
                {
                    // get the nearest
                    currentSeries = actualModel.GetSeriesFromPoint(e.Position, 20);

                    if (currentSeries == null)
                    {
                        if (!LockToInitialSeries)
                        {
                            PlotView.HideTracker();
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
        private static TrackerHitResult? GetNearestHit(Series.Series series, ScreenPoint point, bool snap, bool pointsOnly)
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
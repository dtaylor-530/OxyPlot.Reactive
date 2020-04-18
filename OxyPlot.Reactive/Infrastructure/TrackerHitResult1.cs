#nullable enable

using OxyPlot;
using System.Collections.Generic;
using System.Drawing;

namespace OxyPlotEx.ViewModel
{
    public class TrackerHitResult1 : TrackerHitResult
    {
        public class ValueAndBrush
        {
            public double Value { get; set; }
            public Color Color { get; set; }
        }

        public Dictionary<string, ValueAndBrush> Values { get; private set; }

        public TrackerHitResult1(Dictionary<string, ValueAndBrush> values, TrackerHitResult trackerHitResult)
        {
            this.Values = values;
            this.Index = trackerHitResult.Index;
            this.DataPoint = trackerHitResult.DataPoint;
            this.Item = trackerHitResult.Item;
            this.LineExtents = trackerHitResult.LineExtents;
            this.PlotModel = trackerHitResult.PlotModel;
            this.Position = trackerHitResult.Position;
            this.Series = trackerHitResult.Series;
            this.Text = trackerHitResult.Text;
        }
    }
}

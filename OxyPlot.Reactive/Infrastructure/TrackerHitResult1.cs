#nullable enable

using OxyPlot;
using System.Collections.Generic;
using System.Drawing;

namespace OxyPlot.Reactive.Infrastructure
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
            Values = values;
            Index = trackerHitResult.Index;
            DataPoint = trackerHitResult.DataPoint;
            Item = trackerHitResult.Item;
            LineExtents = trackerHitResult.LineExtents;
            PlotModel = trackerHitResult.PlotModel;
            Position = trackerHitResult.Position;
            Series = trackerHitResult.Series;
            Text = trackerHitResult.Text;
        }
    }
}

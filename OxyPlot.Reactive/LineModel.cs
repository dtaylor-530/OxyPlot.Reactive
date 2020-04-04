


namespace OxyPlotEx.ViewModel
{
    using OxyPlot;
    using OxyPlot.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using e = System.Linq.Enumerable;

    public class LineModel<T> : MultiDateTimeModel<T>
    {

        public LineModel(IDispatcher dispatcher, PlotModel model) : base(dispatcher, model)
        {
        }

        public LineModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : base(dispatcher, model, comparer)
        {
        }


        protected override void AddToSeries(DateTimePoint[] points, string title)
        {

            if (plotModel.Annotations.Any(a => (a is EllipseAnnotation e) && e.Text == title) == false)
            {
                plotModel.Annotations.Add(new EllipseAnnotation { X = 20, Y = 20, Width = 200, Height = 200, Fill = OxyColors.Green, Text = title, Stroke = OxyColors.Black, StrokeThickness = 2 });
            }

            plotModel.PlotType = PlotType.XY;

             base.AddToSeries(points, title);
        }
    }
}


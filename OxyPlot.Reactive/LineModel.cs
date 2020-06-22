#nullable enable
namespace OxyPlot.Reactive
{
    using OxyPlot;
    using OxyPlot.Annotations;
    using OxyPlot.Reactive.Infrastructure;
    using OxyPlot.Reactive.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using e = System.Linq.Enumerable;

    public class LineModel<T> : MultiDateTimeModel<T>
    {

        public LineModel(PlotModel model, IScheduler? scheduler = null) : base( model, scheduler:scheduler)
        {
        }

        public LineModel(PlotModel model, IEqualityComparer<T> comparer, IScheduler? scheduler = null) : base( model, comparer, scheduler: scheduler)
        {
        }


        protected override void AddToSeries(IDateTimeKeyPoint<T>[] items, string title)
        {

            if (plotModel.Annotations.Any(a => a is EllipseAnnotation e && e.Text == title) == false)
            {
                plotModel.Annotations.Add(new EllipseAnnotation { X = 20, Y = 20, Width = 200, Height = 200, Fill = OxyColors.Green, Text = title, Stroke = OxyColors.Black, StrokeThickness = 2 });
            }

            plotModel.PlotType = PlotType.XY;

            base.AddToSeries(items, title);
        }
    }
}


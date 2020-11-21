using OxyPlot.Series;
using ReactivePlot.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ReactivePlot.OxyPlot.Common
{
    using static ColorRepo;

    internal static class OxyFactory
    {
        static OxyFactory()
        {
            //Colors2 = Colors.Value.OrderBy(a => Guid.NewGuid()).Cycle().GetEnumerator();
        }

        public static LineSeries BuildWithMarker(IEnumerable coll, string key)
        {
            var lser = new LineSeries
            {
                Title = key,
                //StrokeThickness = 1,
                Color = GetColor(key),

                ItemsSource = coll,
                //MarkerSize = 0,
                //MarkerType = MarkerType.Plus,
                //MarkerOutline = OxyColors.Blue,
                //MarkerFill = OxyColors.DarkRed,
                //MarkerStroke = OxyColors.Black,
                // MarkerStrokeThickness = 1,
                //MarkerSize = 10
            };

            return lser;
        }

        //public static LineSeries AddMarker(LineSeries lser)
        //{

        //    lser.MarkerSize = 3;
        //    lser.MarkerType = MarkerType.Plus;
        //    //MarkerOutline = OxyColors.Blue,
        //    lser.MarkerFill = OxyColors.DarkRed;
        //    lser.MarkerStroke = OxyColors.Black;
        //    lser.MarkerStrokeThickness = 1;

        //    return lser;
        //}

        public static LineSeries RemoveMarker(LineSeries lser)
        {
            lser.MarkerSize = 0;

            return lser;
        }


        public static LineSeries Build(IEnumerable coll, string key)
        {
            var lser = new LineSeries
            {
                Title = key,
                //StrokeThickness = 1,
                Color = GetColor(key),
                ItemsSource = coll,
            };

            return lser;
        }

        //public static LineSeries BuildWithMarker(IEnumerable<IDataPointProvider> coll, string trackerFormatString, string title)
        //{
        //    var lser = new LineSeries
        //    {
        //        Title = title,
        //        StrokeThickness = 1,
        //        Color = GetColor(title),
        //        MarkerSize = 3,
        //        ItemsSource = coll,
        //        MarkerType = MarkerType.Plus,
        //        MarkerFill = OxyColors.DarkRed,
        //        MarkerStroke = OxyColors.Black,
        //        MarkerStrokeThickness = 1,
        //        TrackerFormatString = trackerFormatString
        //    };

        //    return lser;
        //}


        //public static LineSeries Build(IEnumerable<IDataPointProvider> coll, string trackerFormatString, string title)
        //{
        //    var lser = new LineSeries
        //    {
        //        Title = title,
        //        StrokeThickness = 1,
        //        Color = GetColor(title),
        //        MarkerSize = 3,
        //        ItemsSource = coll,

        //    };

        //    return lser;
        //}

        //public static LineSeries BuildWithMarker(IEnumerable<TimePoint> coll, string key)
        //{
        //    var lser = new LineSeries
        //    {
        //        Title = key,
        //        StrokeThickness = 1,
        //        Color = GetColor(key),
        //        MarkerSize = 3,
        //        ItemsSource = coll,
        //        //MarkerType = MarkerType.Plus,
        //        MarkerFill = OxyColors.DarkRed,
        //        MarkerStroke = OxyColors.Black,
        //        MarkerStrokeThickness = 1,
        //        //DataFieldX = nameof(DateTimePoint.DateTime),
        //        //DataFieldY = nameof(DateTimePoint.Value)
        //    };

        //    return lser;
        //}

        public static LineSeries Build(IEnumerable<TimePoint> coll, string key)
        {
            var lser = new LineSeries
            {
                Title = key,
                //StrokeThickness = 1,
                Color = GetColor(key),
                ItemsSource = coll,
            };

            return lser;
        }

        //public static LineSeries BuildWithMarker<T>(IEnumerable<ITimePoint<T>> coll, string key)
        //{
        //    var lser = new LineSeries
        //    {
        //        Title = key,
        //        StrokeThickness = 1,
        //        Color = GetColor(key),
        //        MarkerSize = 3,
        //        ItemsSource = coll,
        //        MarkerType = MarkerType.Plus,
        //        MarkerFill = OxyColors.DarkRed,
        //        MarkerStroke = OxyColors.Black,
        //        MarkerStrokeThickness = 1,
        //        //DataFieldX = nameof(DateTimePoint.DateTime),
        //        //DataFieldY = nameof(DateTimePoint.Value)
        //    };

        //    return lser;
        //}

        public static LineSeries Build<T>(IEnumerable<ITimePoint<T>> coll, string key)
        {
            var lser = new LineSeries
            {
                Title = key,
                //StrokeThickness = 1,
                Color = GetColor(key),

                ItemsSource = coll,

            };

            return lser;
        }

        //internal static IEnumerable<Series.Series> BuildWithMarker(IOrderedEnumerable<TimeUncertainPoint> points, string key)
        //{
        //    var color = GetColor(key);
        //    yield return new AreaSeries
        //    {
        //        Title = key,
        //        StrokeThickness = 1,

        //        //Color2 = color.ChangeIntensity(2.5),
        //        //Color = color.ChangeIntensity(0.5),

        //        ItemsSource = points,

        //        DataFieldX = nameof(TimeUncertainPoint.DateTime),
        //        DataFieldY = nameof(TimeUncertainPoint.Lower),
        //        DataFieldX2 = nameof(TimeUncertainPoint.DateTime),
        //        DataFieldY2 = nameof(TimeUncertainPoint.Upper),
        //        MarkerSize = 3,
        //        MarkerType = MarkerType.Plus,
        //        MarkerFill = OxyColors.DarkRed,
        //        MarkerStroke = OxyColors.Black,
        //        MarkerStrokeThickness = 1,
        //    };

        //    yield return new LineSeries
        //    {
        //        Title = key,
        //        StrokeThickness = 1,
        //        Color = color,

        //        ItemsSource = points.Select(p => new TimePoint(p.DateTime, p.Value)),
        //        MarkerSize = 3,
        //        MarkerType = MarkerType.Plus,
        //        MarkerFill = OxyColors.DarkRed,
        //        MarkerStroke = OxyColors.Black,
        //        MarkerStrokeThickness = 1,
        //    };
        //}


        internal static IEnumerable<Series> Build(IOrderedEnumerable<TimeErrorPoint> points, string key)
        {
            var color = GetColor(key);
            yield return new AreaSeries
            {
                Title = key,
                //StrokeThickness = 1,

                //Color2 = color.ChangeIntensity(2.5),
                //Color = color.ChangeIntensity(0.5),

                ItemsSource = points,

                DataFieldX = nameof(TimeErrorPoint.DateTime),
                DataFieldY = nameof(TimeErrorPoint.Lower),
                DataFieldX2 = nameof(TimeErrorPoint.DateTime),
                DataFieldY2 = nameof(TimeErrorPoint.Upper),
            };

            yield return new LineSeries
            {
                Title = key,
                //StrokeThickness = 1,
                Color = color,
                ItemsSource = points.Select(p => new TimePoint(p.DateTime, p.Value)),

            };
        }

        internal static Series BuildBoxPlot(BoxPlotItem[] points, string title)
        {
            return new BoxPlotSeries
            {
                ItemsSource = points,
                Title = title,
                Stroke = GetColor(title)
            };
        }


        internal static Series BuildError(ErrorColumnItem[] points, string title)
        {
            return new ErrorColumnSeries
            {
                ErrorStrokeThickness = 4,
                ErrorWidth = 0,
                StrokeThickness = 1,
                Title = title,
                ItemsSource = points,
                XAxisKey = "y",
                YAxisKey = "x",
            };
        }
    }
}
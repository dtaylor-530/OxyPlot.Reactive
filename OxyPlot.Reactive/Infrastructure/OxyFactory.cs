using OxyPlot.Reactive.Model;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Infrastructure
{
    using static ColorRepo;

    static class OxyFactory
    {

        static OxyFactory()
        {
            //Colors2 = Colors.Value.OrderBy(a => Guid.NewGuid()).Cycle().GetEnumerator();
        }

        public static LineSeries Build(IEnumerable<DataPoint> coll, string key)
        {
            var lser = new LineSeries
            {
                Title = key,
                StrokeThickness = 1,
                Color = GetColor(key),
                MarkerSize = 3,
                ItemsSource = coll,
                MarkerType = MarkerType.Plus
            };

            return lser;
        }

        public static LineSeries Build(IEnumerable<IDataPointProvider> coll, string trackerFormatString, string title)
        {
            var lser = new LineSeries
            {
                Title = title,
                StrokeThickness = 1,
                Color = GetColor(title),
                MarkerSize = 3,
                ItemsSource = coll,
                MarkerType = MarkerType.Plus,
                TrackerFormatString = trackerFormatString
            };

            return lser;
        }


        public static LineSeries Build(IEnumerable<DateTimePoint> coll, string key)
        {
            var lser = new LineSeries
            {
                Title = key,
                StrokeThickness = 1,
                Color = GetColor(key),
                MarkerSize = 3,
                ItemsSource = coll,
                MarkerType = MarkerType.Plus,

                //DataFieldX = nameof(DateTimePoint.DateTime),
                //DataFieldY = nameof(DateTimePoint.Value)
            };

            return lser;
        }


        public static LineSeries Build<T>(IEnumerable<IDateTimeKeyPoint<T>> coll, string key)
        {
            var lser = new LineSeries
            {
                Title = key,
                StrokeThickness = 1,
                Color = GetColor(key),
                MarkerSize = 3,
                ItemsSource = coll,
                MarkerType = MarkerType.Plus,

                //DataFieldX = nameof(DateTimePoint.DateTime),
                //DataFieldY = nameof(DateTimePoint.Value)
            };

            return lser;
        }

        internal static IEnumerable<Series.Series> Build(IOrderedEnumerable<DateTimeUncertainPoint> points, string key)
        {
            var color = GetColor(key);
            yield return new AreaSeries
            {
                Title = key,
                StrokeThickness = 1,

                //Color2 = color.ChangeIntensity(2.5),
                //Color = color.ChangeIntensity(0.5),
                MarkerSize = 3,
                ItemsSource = points,
                MarkerType = MarkerType.Plus,
                DataFieldX = nameof(DateTimeUncertainPoint.DateTime),
                DataFieldY = nameof(DateTimeUncertainPoint.Lower),
                DataFieldX2 = nameof(DateTimeUncertainPoint.DateTime),
                DataFieldY2 = nameof(DateTimeUncertainPoint.Upper)
            };

            yield return new LineSeries
            {
                Title = key,
                StrokeThickness = 1,
                Color = color,
                MarkerSize = 3,
                ItemsSource = points.Select(p => new DateTimePoint(p.DateTime, p.Value)),
                MarkerType = MarkerType.Plus,
                //DataFieldX = nameof(DateTimePoint.DateTime),
                //DataFieldY = nameof(DateTimePoint.Value)
            };
        }

        internal static Series.Series Build(BoxPlotItem[] points, string title)
        {
            return new BoxPlotSeries
            {
                ItemsSource = points,
                Title = title,
                Stroke = GetColor(title)
            };
        }


        //internal static Series.Series Build(ErrorBarItem[] points, string title)
        //{
        //    return new ErrorBarSeries
        //    {
        //        ErrorStrokeThickness = 4,
        //        ErrorWidth = 0,
        //        StrokeThickness = 1,
        //        Title = title,
        //        ItemsSource = points,
        //        XAxisKey = "y",
        //        YAxisKey = "x"
        //    };

        //}
        
        internal static Series.Series Build(ErrorColumnItem[] points, string title)
        {
            return new ErrorColumnSeries
            {
                ErrorStrokeThickness = 4,
                ErrorWidth = 0,
                StrokeThickness = 1,
                Title = title,
                ItemsSource = points,
                XAxisKey = "y",
                YAxisKey = "x"
            };
        }
    }
}


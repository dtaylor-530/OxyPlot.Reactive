using Endless;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxyPlotEx.ViewModel
{
    class OxyFactory
    {
        private static Dictionary<string, string> ColorsMatch = new Dictionary<string, string>();
        private static IEnumerator<KeyValuePair<string, string>> Colors2;

        static OxyFactory()
        {
            Colors2 = Colors.Value.OrderBy(a => Guid.NewGuid()).Cycle().GetEnumerator();
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
                MarkerType = OxyPlot.MarkerType.Plus,

                //DataFieldX = nameof(DateTimePoint.DateTime),
                //DataFieldY = nameof(DateTimePoint.Value)
            };

            return lser;
        }

        internal static IEnumerable<Series> Build(IOrderedEnumerable<DateTimeUncertainPoint> points, string key)
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
                MarkerType = OxyPlot.MarkerType.Plus,
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
                MarkerType = OxyPlot.MarkerType.Plus,
                //DataFieldX = nameof(DateTimePoint.DateTime),
                //DataFieldY = nameof(DateTimePoint.Value)
            };
        }

        internal static Series Build(BoxPlotItem[] points, string title)
        {
            return new BoxPlotSeries
            {
                ItemsSource = points,
                Title = title,
                Stroke = GetColor(title)
            };
        }


        internal static Series Build(ErrorColumnItem[] points, string title)
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

        private static OxyColor GetColor(string key)
        {
            var value = Math.Abs(key.GetHashCode()) % Colors.Value.Count;
            return OxyColor.Parse(Colors.Value.ElementAt(value).Value);
        }

        private static OxyColor NextColor()
        {
            Colors2.MoveNext(); return OxyColor.Parse(Colors2.Current.Value);
        }


        /// <summary>
        /// A nicer color palette for the web.
        /// https://github.com/mrmrs/colors
        /// </summary>
        /// <returns></returns>
        private static OxyColor GetColorRandom(string key)
        {
            if (ColorsMatch.ContainsKey(key) == false)
            {
                Colors2.MoveNext();
                ColorsMatch[key] = Colors2.Current.Key;
            }
            return OxyColor.Parse(Colors.Value[ColorsMatch[key]]);
        }


        /// <summary>
        /// A nicer color palette for the web.
        /// https://github.com/mrmrs/colors
        /// </summary>
        private static Lazy<Dictionary<string, string>> Colors = new Lazy<Dictionary<string, string>>(() =>
            new Dictionary<string, string> {
            { "navy", "#001F3F"} ,

             { "blue", "#0074D9"} ,
              { "aqua", "#7FDBFF"} ,
                                  { "teal", "#39CCCC"} ,
                                  { "olive", "#3D9970"} ,
                                  { "green", "#2ECC40"} ,
                                  { "lime", "#01FF70"} ,
                                  {  "yellow", "#FFDC00"} ,
                                  {  "orange", "#FF851B"} ,
                                  { "red", "#FF4136"} ,
               { "fuchsia", "#F012BE"} ,

             { "purple", "#B10DC9"} ,

             { "maroon", "#85144B"} ,
              //{ "white", "#FFFFFF"} ,
                                  //{ "gray", "#AAAAAA"} ,
                                  { "silver", "#DDDDDD"} ,
                                  { "black", "#111111"}

                              });

    }
}


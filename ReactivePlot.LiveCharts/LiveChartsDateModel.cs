using LiveCharts;
using System;
using System.Linq;

namespace UtilityWpf.ViewModel.Livecharts
{
    public class DateModel
    {
        public System.DateTime DateTime { get; set; }
        public double Value { get; set; }
    }

    public static class DateModelHelper
    {
        public static void AddPoints(this SeriesCollection col, string title, params Tuple<DateTime, double>[] vals)
        {
            var seriesView = col.Where(l => l.Title == title).FirstOrDefault();

            var points = vals.Select(yt => new LiveCharts.Defaults.ObservablePoint(yt.Item1.Ticks, (double)yt.Item2));

            if (seriesView == null)
                col.Add(new LiveCharts.Wpf.LineSeries
                {
                    Title = title,
                    Values = new ChartValues<LiveCharts.Defaults.ObservablePoint>(points)
                });
            else
            {
                seriesView.Values.AddRange(points);
            }
        }
    }
}
using OxyPlotEx.DemoApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OxyPlot.Reactive.DemoApp.Factory
{
    class DataSource
    {
        public static IObservable<KeyValuePair<string, (DateTime, double)>> Observe1000()
        {
            var array = new DataFactory().GetLine().Take(1000).Select((o, i) =>
            {
                return new KeyValuePair<string, (DateTime, double)>(o.Key, (DateTime.UnixEpoch.AddYears(i), o.Value));
            }).ToArray();

            return array.ToObservable();
        }

        public static IObservable<KeyValuePair<string, (DateTime, double)>> Observe20()
        {
            return Observe1000().Take(20);
        }

        public static DateTime ToDateTime(double d) => DateTime.UnixEpoch.AddDays((int)d);

        public static TimeSpan ToTimeSpan(double d) => DateTime.UnixEpoch.AddDays((int)d) - DateTime.UnixEpoch;

        public static double FromDateTime(DateTime d) => (d - DateTime.UnixEpoch).TotalDays;
    }
}

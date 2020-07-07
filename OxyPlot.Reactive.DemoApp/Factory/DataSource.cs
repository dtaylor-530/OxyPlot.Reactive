using OxyPlotEx.DemoApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OxyPlot.Reactive.DemoApp.Factory
{
    class DataSource
    {
        private static Lazy<KeyValuePair<string, KeyValuePair<DateTime, double>>[]> array;

        static DataSource()
        {
            array = new Lazy<KeyValuePair<string, KeyValuePair<DateTime, double>>[]>(() =>
             {
                 var array = new DataFactory().GetLine().Take(1000).Select((o, i) =>
                 {
                     return new KeyValuePair<string, KeyValuePair<DateTime, double>>(o.Key, KeyValuePair.Create(DateTime.UnixEpoch.AddYears(i), o.Value));
                 }).ToArray();

                 return array;
             });
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe1000()
        {
            return array.Value.ToObservable();
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe20()
        {
            return array.Value.Take(20).ToObservable();
        }


        public static DateTime ToDateTime(double d) => DateTime.UnixEpoch.AddDays((int)d);

        public static TimeSpan ToTimeSpan(double d) => DateTime.UnixEpoch.AddDays((int)d) - DateTime.UnixEpoch;

        public static double FromDateTime(DateTime d) => (d - DateTime.UnixEpoch).TotalDays;
    }
}

using Endless;
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
                     return KeyValuePair.Create(o.Key, KeyValuePair.Create(DateTime.UnixEpoch.AddYears(i), o.Value));
                 }).ToArray();

                 return array;
             });
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe1000()
        {
            return array.Value.ToObservable();
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe1000PlusMinus()
        {
            return array.Value.ToObservable().Select(a =>
            {
                var kvp = KeyValuePair.Create(a.Value.Key, Enumerable.Range(-1, 3).Random() * a.Value.Value);
                var tt = KeyValuePair.Create(a.Key, kvp);
                return tt;
            });
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe20()
        {
            return array.Value.Take(20).ToObservable();
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe3()
        {
            return array.Value.Take(3).ToObservable();
        }


        public static DateTime ToDateTime(double d) => DateTime.UnixEpoch.AddDays((int)d);

        public static TimeSpan ToTimeSpan(double d) => DateTime.UnixEpoch.AddDays((int)d) - DateTime.UnixEpoch;

        public static double FromDateTime(DateTime d) => (d - DateTime.UnixEpoch).TotalDays;
    }
}

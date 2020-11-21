using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ReactivePlot.Data.Factory
{
    public static class TimeDataSource
    {
        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe1000PlusMinus() => DataSource.Observe1000PlusMinus().ToDateTimeObservable();

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe1000() => DataSource.Observe1000().ToDateTimeObservable();

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe20() => DataSource.Observe20().ToDateTimeObservable();

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> Observe3() => DataSource.Observe3().ToDateTimeObservable();

        public static DateTime ToDateTime(double d) => DateTime.UnixEpoch.AddDays((int)d);

        public static TimeSpan ToTimeSpan(double d) => DateTime.UnixEpoch.AddDays((int)d) - DateTime.UnixEpoch;

        public static double FromDateTime(DateTime d) => (d - DateTime.UnixEpoch).TotalDays;

        public static IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> ToDateTimeObservable(this IObservable<KeyValuePair<string, KeyValuePair<int, double>>> observable)
            => observable.Select(a => KeyValuePair.Create(a.Key, KeyValuePair.Create(DateTime.UnixEpoch.AddYears(a.Value.Key), a.Value.Value)));
    }
}
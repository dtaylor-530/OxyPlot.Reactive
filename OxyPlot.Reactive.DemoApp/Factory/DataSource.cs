using OxyPlotEx.DemoApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace OxyPlot.Reactive.DemoApp.Factory
{
    class DataSource
    {
        public static IObservable<KeyValuePair<string, (DateTime, double)>> Observe()
        {
            DateTime now = DateTime.Now;
            var get2 = new DataFactory().GetLine().GetEnumerator();

            var observable1 = Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(t =>
            {
                get2.MoveNext();
                return get2.Current;
            }).Skip(1);

            var obs1 = observable1.Select((o, i) =>
            {
                return new KeyValuePair<string, (DateTime, double)>(o.Key, (now.AddHours(i), o.Value));
            });
            return obs1;
        }

        public static IObservable<KeyValuePair<string, (DateTime, double)>> Observe2()
        {
            var array = new DataFactory().GetLine().Skip(100).Take(20).Select((o, i) =>
            {
                return new KeyValuePair<string, (DateTime, double)>(o.Key, (DateTime.UnixEpoch.AddYears(i), o.Value));
            }).ToArray();
            
            return array.ToObservable();

        }
    }
}

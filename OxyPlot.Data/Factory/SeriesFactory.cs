using OxyPlot.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace OxyPlot.Data.Factory
{
    internal class SeriesFactory
    {
        private static double equation(int i) => Math.Sin(i * 3.14 * 10 / 180);

        private static Random r = new Random();

        public static IEnumerable<KeyValuePair<Tuple<string, string>, Tuple<DateTime, double>>> ValuesStandard(Dictionary<string, Tuple<double, int>> signals, string[] elements, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var dictMeas = new Dictionary<string, Dictionary<string, Tuple<DateTime, double>>>();
                var actualValue = MeasurementGenerator.Generate(i, equation);

                foreach (var elem in elements)
                {
                    yield return new KeyValuePair<Tuple<string, string>, Tuple<DateTime, double>>(Tuple.Create(elem, "Actual"), actualValue);
                    foreach (var kvp in RandomVariant.Generate(actualValue.Item2, signals, r, i))
                        //dict[elem][key].Add(dictMeas[elem][key]);
                        yield return new KeyValuePair<Tuple<string, string>, Tuple<DateTime, double>>(Tuple.Create(elem, kvp.Key), kvp.Value);
                }
            }
        }

        public static IEnumerable<KeyValuePair<Tuple<string, string>, Tuple<DateTime, double, double, double>>> ValuesWithDeviation(Dictionary<string, Tuple<double, int>> signals, /*string[] elements, */int count)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (var kvp in ValuesWithDeviation(i, signals/*, elements*/))
                    yield return kvp;
            }
        }

        public static IObservable<IEnumerable<KeyValuePair<Tuple<string, string>, Tuple<DateTime, double, double, double>>>> ValuesWithDeviation(Dictionary<string, Tuple<double, int>> signals, /*string[] elements,*/ int count, TimeSpan ts)
        {
            Func<int, IEnumerable<KeyValuePair<Tuple<string, string>, Tuple<DateTime, double, double, double>>>> x = (i) => ValuesWithDeviation(i, signals/*, elements*/);

            return MakeTimedObservable(x, TimeSpan.FromSeconds(1), count);
        }

        //dict[elem][key].Add(dictMeas[elem][key]);

        public static IEnumerable<KeyValuePair<Tuple<string, string>, Tuple<DateTime, double, double, double>>> ValuesWithDeviation(int i, Dictionary<string, Tuple<double, int>> signals/*, string[] elements*/)
        {
            var dictMeas = new Dictionary<string, Dictionary<string, Tuple<DateTime, double, double, double>>>();
            var actualValue = MeasurementGenerator.Generate(i, equation);

            //foreach (var elem in elements)
            //{
            yield return new KeyValuePair<Tuple<string, string>, Tuple<DateTime, double, double, double>>(Tuple.Create("a", "Actual"), Tuple.Create(actualValue.Item1, actualValue.Item2, 0d, 0d));
            foreach (var kvp in RandomVariant.GenerateWithDeviation(actualValue.Item2, signals, r, i))
                yield return new KeyValuePair<Tuple<string, string>, Tuple<DateTime, double, double, double>>(Tuple.Create("a", kvp.Key), Tuple.Create(actualValue.Item1, kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3));
            //}
        }

        public static IObservable<T> MakeTimedObservable<T>(Func<T> act, TimeSpan ts, int? limit = null, bool skipinitial = false, IScheduler scheduler = null)
        {
            return Observable.Create<T>(observer =>
            {
                if (!skipinitial)
                    observer.OnNext(act());

                IObservable<long> lgs = Observable.Interval(ts);
                lgs = limit != null ? lgs.Take((int)limit) : lgs;
                lgs = scheduler != null ? lgs.ObserveOn(scheduler) : lgs;

                return lgs.Subscribe(a => observer.OnNext(act()), ex =>
                {
                    try { observer.OnError(ex); }
                    catch
                    {
                        Console.WriteLine(ex.Message);
                    }
                }, () => Console.WriteLine("Observer has unsubscribed from timed observable"));
            });
        }

        public static IObservable<T> MakeTimedObservable<T>(Func<int, T> act, TimeSpan ts, int? limit = null, bool skipinitial = false, IScheduler scheduler = null)
        {
            int i = 0;
            return Observable.Create<T>(observer =>
            {
                if (!skipinitial)
                    observer.OnNext(act(1));

                IObservable<long> lgs = Observable.Interval(ts);
                lgs = limit != null ? lgs.Take((int)limit) : lgs;
                lgs = scheduler != null ? lgs.ObserveOn(scheduler) : lgs;

                return lgs.Subscribe(a => { i++; observer.OnNext(act(i)); }, ex =>
                {
                    try { observer.OnError(ex); }
                    catch
                    {
                        Console.WriteLine(ex.Message);
                    }
                }, () => Console.WriteLine("Observer has unsubscribed from timed observable"));
            });
        }
    }
}
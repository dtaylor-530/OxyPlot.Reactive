using Endless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ReactivePlot.Data.Factory
{
    public static class DataSource
    {
        private static Lazy<KeyValuePair<string, KeyValuePair<int, double>>[]> array;
        static Random random = new Random();
        static Troschuetz.Random.Distributions.Continuous.NormalDistribution trandom = new Troschuetz.Random.Distributions.Continuous.NormalDistribution(3, 0.5);
        static Troschuetz.Random.Distributions.Continuous.NormalDistribution trandom2 = new Troschuetz.Random.Distributions.Continuous.NormalDistribution(7, 2);

        static DataSource()
        {
            array = new Lazy<KeyValuePair<string, KeyValuePair<int, double>>[]>(() =>
            {
                var array = new DataFactory().GetLine().Take(1000).Select((o, i) =>
                {
                    return KeyValuePair.Create(o.Key, KeyValuePair.Create(i, o.Value));
                }).ToArray();

                return array;
            });
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<int, double>>> Observe1000()
        {
            return array.Value.ToObservable();
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<int, double>>> Observe1000PlusMinus()
        {
            return array.Value.ToObservable().Select(a =>
            {
                var kvp = KeyValuePair.Create(a.Value.Key, Enumerable.Range(-1, 3).Random() * a.Value.Value);
                var tt = KeyValuePair.Create(a.Key, kvp);
                return tt;
            });
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<double, double>>> Observe1000XYPlusMinus()
        {
            return array.Value.ToObservable().Select(a =>
            {
                var kvp = KeyValuePair.Create(trandom.NextDouble(), trandom2.NextDouble());
                var tt = KeyValuePair.Create(a.Key, kvp);
                return tt;
            });
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<int, double>>> Observe20()
        {
            return array.Value.Take(20).ToObservable();
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<int, double>>> Observe3()
        {
            return array.Value.Take(3).ToObservable();
        }

        public static IObservable<KeyValuePair<string, KeyValuePair<double, double>>> ToDoubles(this IObservable<KeyValuePair<string, KeyValuePair<int, double>>> observable)
        {
            return observable.Select(a => KeyValuePair.Create(a.Key, KeyValuePair.Create((double)a.Value.Key, a.Value.Value)));
        }
    }
}
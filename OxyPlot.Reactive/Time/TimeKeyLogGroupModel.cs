#nullable enable

using MoreLinq;
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;

namespace OxyPlot.Reactive
{

    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeKeyLogValueGroupModel : TimeKeyLogGroupModel<double>
    {

        public TimeKeyLogValueGroupModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override string CreateGroupKey(IKeyPoint<double, DateTime, double> val)
        {
            if (Power.HasValue == false)
            {
                return default(double).ToString();
            }

            int v = (int)Math.Log(val.Key, Power.Value);

            var min = Math.Pow(Power.Value, v);
            var max = Math.Pow(Power.Value, v + 1);
            return $"{min:N} - {max:N}";
        }



    }


    public class TimeKeyLogGroupModel<TKey> : TimeKeyLogGroupModel<TKey, ITimePoint<TKey>>
    {
        public TimeKeyLogGroupModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

        protected override IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<string, ITimePoint<TKey>>> collection)
        {
            return collection
   .Select(a => a.Value)
   .Select(a => { return a; })
   .Scan(seed: default(ITimePoint<TKey>), (a, b) => CreatePoint(a, b))
   .Skip(1)
   .Cast<ITimePoint<TKey>>();
        }
    }

    /// <summary>
    /// Groups values by the Value, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeKeyLogGroupModel<TKey, TPoint> : TimeKeyGroupModel<string, TKey, TPoint, TPoint>, IObserver<double>
        where TPoint : ITimePoint<TKey>
    {
        protected Subject<double> powerSubject = new Subject<double>();

        public TimeKeyLogGroupModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            powerSubject
                .Subscribe(a =>
                {
                    Power = a;
                    model.Series.Clear();
                    model.InvalidatePlot(true);
                    refreshSubject.OnNext(Unit.Default);
                });

            this.OnNext(new Comparer());
        }



        public double? Power { get; private set; }


        protected override string CreateGroupKey(IKeyPoint<TKey, DateTime, double> val)
        {
            if (Power.HasValue == false)
            {
                return val.Key?.ToString();
            }

            int v = (int)Math.Log(val.Value, Power.Value);

            var min = Math.Pow(Power.Value, v);
            var max = Math.Pow(Power.Value, v + 1);
            return $"{min:N} - {max:N}";
        }

        public IDisposable Subscribe(IObserver<double> observer)
        {
            return powerSubject.Subscribe(observer);
        }

        public void OnNext(double value)
        {
            powerSubject.OnNext(value);
        }

        public class Comparer : IComparer<string>
        {
            const string pattern = @"([\.\d]+) - ([\.\d]+)";
            public  int Compare(string x, string y)
            {
                return double.Parse(Regex.Match(x, pattern).Groups[1].Captures[0].Value)
                    .CompareTo(double.Parse(Regex.Match(y, pattern).Groups[1].Captures[0].Value));
            }
        }
    }
}
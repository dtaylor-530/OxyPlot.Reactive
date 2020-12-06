#nullable enable

using MoreLinq;
using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;

namespace ReactivePlot.Time
{

    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeLogGroupKeyModel : TimeLogGroupValueModel<double>
    {

        public TimeLogGroupKeyModel(IMultiPlotModel<ITimePoint<double>> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override string CreateGroupKey(IKeyPoint<double, DateTime, double> val)
        {
            return Power.HasValue == false ?
                default(double).ToString() :
                GroupKeyFactory.Create(val.Value, Power.Value);
        }
    }


    public class TimeLogGroupValueModel<TKey> : TimeLogGroupValueModel<TKey, ITimePoint<TKey>>
    {
        public TimeLogGroupValueModel(IMultiPlotModel<ITimePoint<TKey>> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimePoint<TKey> CreateNewPoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

   //     protected override IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<string, ITimePoint<TKey>>> collection)
   //     {
   //         return collection
   //.Select(a => a.Value)
   //.Select(a => { return a; })
   //.Scan(seed: default(ITimePoint<TKey>), (a, b) => CreateNewPoint(a, b))
   //.Skip(1)
   //.Cast<ITimePoint<TKey>>();
   //     }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

        protected override string GetKey(ITimePoint<TKey> item)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Groups values by the Value, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TimeLogGroupValueModel<TKey, TPoint> : TimeKeyGroupModel<string, TKey, TPoint, TPoint>, IObserver<double>
        where TPoint : ITimePoint<TKey>
    {
        protected Subject<double> powerSubject = new Subject<double>();

        public TimeLogGroupValueModel(IMultiPlotModel<TPoint> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            powerSubject
                .Subscribe(a =>
                {
                    Power = a;
                    model.Clear();
                    model.Invalidate(true);
                    refreshSubject.OnNext(Unit.Default);
                });

            this.OnNext(new Comparer());
        }

        protected override TPoint CreatePoint(TPoint xy0, TPoint xy)
        {
            return CreateNewPoint(xy0, xy);
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
            public int Compare(string x, string y)
            {
                return double.Parse(Regex.Match(x, pattern).Groups[1].Captures[0].Value)
                    .CompareTo(double.Parse(Regex.Match(y, pattern).Groups[1].Captures[0].Value));
            }
        }
    }
}
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
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    /// <summary>
    /// Groups values by the Value, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeKeyDoubleGroupModel<TKey> : TimeKeyGroupModel<string, TKey>, IObserver<double>
    {
        //protected Subject<double> groupRangeSubject = new Subject<double>();
        protected Subject<double> powerSubject = new Subject<double>();

        public TimeKeyDoubleGroupModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            powerSubject
                .Subscribe(async a =>
                {
                    Power = a;
                    await Task.Run(() =>
                    {
                        lock (DataPoints)
                        {
                            var dataPoints = DataPoints.SelectMany(a => a.Value).GroupBy(a => CreateGroupKey(a)).ToArray();
                            DataPoints.Clear();
                            foreach (var points in dataPoints)
                            {
                                DataPoints[points.Key] = CreateCollection();

                                foreach (var point in points)
                                {
                                    DataPoints[points.Key].Add(point);
                                }
                            }
                        }
                    });
                    model.Series.Clear();
                    model.InvalidatePlot(true);
                    refreshSubject.OnNext(Unit.Default);
                });
        }

  

        public double? Power { get; private set; }

        protected override string CreateGroupKey(ITimePoint<TKey> val)
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
}
#nullable enable

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
    /// Groups each series individually
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeKeyDoubleGroupModel : TimeKeyGroupModel<string>, IObserver<double>
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

        //protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        //{
        //    return new TimePoint<double>(xy.Var, xy.Value, CreateKey(xy));

        //}


        protected override string CreateGroupKey(ITimePoint<string> val)
        {
            if (Power.HasValue == false)
            {
                return val.Key;
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

        public override void OnNext(KeyValuePair<string, ITimePoint<string>> item)
        {
            lock (list)
                list.Add(KeyValuePair.Create(CreateGroupKey(item.Value), CreatePoint(null, item.Value)));
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(double value)
        {
            powerSubject.OnNext(value);
        }
    }

    ///// <summary>
    ///// Groups each series individually
    ///// </summary>
    ///// <typeparam name="TKey"></typeparam>
    //public class TimeKeyDouble2GroupModel : TimeKeyGroupModel<string, double>, IObserver<double>
    //{
    //    //protected Subject<double> groupRangeSubject = new Subject<double>();
    //    protected Subject<double> powerSubject = new Subject<double>();

    //    public TimeKeyDouble2GroupModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
    //    {
    //        powerSubject
    //            .Subscribe(async a =>
    //            {
    //                Power = a;
    //                await Task.Run(() =>
    //                {
    //                    lock (DataPoints)
    //                    {
    //                        var dataPoints = DataPoints.SelectMany(a => a.Value).GroupBy(a => CreateGroupKey(a)).ToArray();
    //                        DataPoints.Clear();
    //                        foreach (var points in dataPoints)
    //                        {
    //                            DataPoints[points.Key] = CreateCollection();

    //                            foreach (var point in points)
    //                            {
    //                                DataPoints[points.Key].Add(point);
    //                            }
    //                        }
    //                    }
    //                });
    //                model.Series.Clear();
    //                model.InvalidatePlot(true);
    //                refreshSubject.OnNext(Unit.Default);
    //            });
    //    }

    //    public double? Power { get; private set; }

    //    //protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
    //    //{
    //    //    return new TimePoint<double>(xy.Var, xy.Value, CreateKey(xy));

    //    //}


    //    protected override string CreateGroupKey(ITimePoint<double> val)
    //    {
    //        if (Power.HasValue == false)
    //        {
    //            return val.Key.ToString("N");
    //        }

    //        int v = (int)Math.Log(val.Value, Power.Value);

    //        var min = Math.Pow(Power.Value, v);
    //        var max = Math.Pow(Power.Value, v + 1);
    //        return $"{min:N} - {max:N}";
    //    }

    //    public IDisposable Subscribe(IObserver<double> observer)
    //    {
    //        return powerSubject.Subscribe(observer);
    //    }

    //    public override void OnNext(KeyValuePair<string, ITimePoint<double>> item)
    //    {
    //        lock (list)
    //            list.Add(KeyValuePair.Create(CreateGroupKey(item.Value), CreatePoint(null, item.Value)));
    //        refreshSubject.OnNext(Unit.Default);
    //    }

    //    public void OnNext(double value)
    //    {
    //        powerSubject.OnNext(value);
    //    }
    //}
}
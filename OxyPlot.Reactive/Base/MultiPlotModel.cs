#nullable enable

using Kaos.Collections;
using MoreLinq;
using OxyPlot.Reactive.Common;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    public abstract class MultiPlotModel<TKey, TVar, TType> : MultiPlotModel<TKey, TVar, TType, TType>, IObserver<int>

        where TType : Model.IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {
        public MultiPlotModel(PlotModel model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, max, min, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<TType> ToDataPoints(IEnumerable<KeyValuePair<TKey, TType>> collection) =>
            collection
            .Select(a => a.Value)
            .Select(a => { return a; })
            .Scan(seed: default(TType), (a, b) => CreatePoint(a, b))
            .Skip(1);

    }

    public abstract class MultiPlotModel<TKey, TVar, TType, TType3> : MultiPlotModel<TKey, TKey, TVar, TType, TType3>, IObserver<int>
        where TType3 : TType
        where TType : Model.IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {
        public MultiPlotModel(PlotModel model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, max, min, comparer, scheduler: scheduler)
        {
        }
    }

    public abstract class MultiPlotModel<TGroupKey, TKey, TVar, TType, TType3> : MultiPlotModel2Base<TGroupKey, TKey, TVar, TType, TType3>, IObservable<TType3[]>, IObserver<int>, IObservable<Exception>
        where TType3 : TType
        where TType : Model.IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {
        protected readonly Subject<TType3> subject = new Subject<TType3>();
        protected readonly Collection<KeyValuePair<TGroupKey, TType>> temporaryCollection = new Collection<KeyValuePair<TGroupKey, TType>>();
        protected readonly Subject<TType3[]> pointsSubject = new Subject<TType3[]>();
        protected readonly Subject<Exception> exceptionSubject = new Subject<Exception>();
        protected int? takeLastCount;

        public MultiPlotModel(PlotModel model, TVar max, TVar min, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            this.Max = max;
            this.Min = min;
        }

        protected TVar Min { get; set; }
        protected TVar Max { get; set; }

        protected override sealed async void Refresh(IList<Unit> units)
        {
            await AddToDataPointsAsync();

            _ = (this as IMixedScheduler).ScheduleAction(async () =>
            {
                KeyValuePair<TGroupKey, ICollection<TType>>[]? dataPoints;

                this.PreModify();

                lock (DataPoints)
                    dataPoints = DataPoints.ToArray();

                await AddAllPointsToSeries(dataPoints);

                try
                {
                    plotModel.InvalidatePlot(true);
                }
                catch (Exception e)
                {
                    exceptionSubject.OnNext(e);
                    refreshSubject.OnNext(Unit.Default);
                }
            });
        }

        protected virtual async Task AddToDataPointsAsync()
        {
            await Task.Run(() =>
            {
                lock (temporaryCollection)
                {
                    if (temporaryCollection.Any())
                    {
                        AddToDataPoints(temporaryCollection.ToArray());
                        temporaryCollection.Clear();
                    }
                }
            });
        }

        protected virtual async Task AddAllPointsToSeries(KeyValuePair<TGroupKey, ICollection<TType>>[] dataPoints)
        {
            foreach (var keyValue in dataPoints)
            {
                _ = await Task.Run(() =>
                {
                    return CreateSingle(keyValue).ToArray();
                }).ContinueWith(async points =>
                {
                    AddToSeries(await points, keyValue.Key?.ToString() ?? string.Empty);
                });
            }

            if (showAll || pointsSubject.HasObservers)
            {
                _ = await Task.Run(() =>
                {
                    return CreateMany(dataPoints).ToArray();
                }).ContinueWith(async points =>
                {
                    var taskPoints = await points;

                    if (showAll)
                        AddToSeries(taskPoints, "All");

                    if (pointsSubject.HasObservers)
                    {
                        pointsSubject.OnNext(taskPoints);
                    }
                });
            }
        }

        protected virtual IEnumerable<TType3> CreateSingle(KeyValuePair<TGroupKey, ICollection<TType>> keyValue)
        {
            return Create(Flatten(keyValue.Value, keyValue.Key));
        }

        protected virtual IEnumerable<TType3> CreateMany(IEnumerable<KeyValuePair<TGroupKey, ICollection<TType>>> keyValues)
        {
            return Create(Flatten(keyValues.SelectMany(a => a.Value)));
        }

        private IEnumerable<KeyValuePair<TGroupKey, TType>> Flatten(IEnumerable<TType> value, TGroupKey key = default)
        => value.ToArray().Select(c => KeyValuePair.Create(key, c));

        protected virtual IEnumerable<TType3> Create(IEnumerable<KeyValuePair<TGroupKey, TType>> value)
        {
            return takeLastCount.HasValue ?
                                 Enumerable.TakeLast(ToDataPoints(value), takeLastCount.Value) :
                                 ToDataPoints(value);
        }

        protected override void AddToDataPoints(IEnumerable<KeyValuePair<TGroupKey, TType>> items)
        {
            Min = CalculateMin(items);
            Max = CalculateMax(items);
            base.AddToDataPoints(items);
        }

        protected override ICollection<TType> CreateCollection()
        {
            return new RankedSet<TType>(Comparer<TType>.Create((a, b) => a.Var.CompareTo(b.Var)));
        }

        protected abstract TVar CalculateMin(IEnumerable<KeyValuePair<TGroupKey, TType>> items);

        protected abstract TVar CalculateMax(IEnumerable<KeyValuePair<TGroupKey, TType>> items);

        protected virtual void PreModify()
        {
        }

        protected virtual void AddToSeries(TType3[] items, string title)
        {
            lock (plotModel)
            {
                if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
                {
                    series = OxyFactory.BuildWithMarker(items, title);

                    series
                        .ToMouseDownEvents()
                        .Select(args => OxyMouseDownAction(args, series, items))
                        .Subscribe(subject.OnNext);

                    plotModel.Series.Add(series);
                }
                if (series is LineSeries lSeries)
                {
                    //var count = series.ItemsSource.Count();
                    //lSeries.MarkerSize = (int)(5/ (1 + (Math.Log10(count)))) - 1;
                    //if (count > 100)
                    //    lSeries.MarkerStrokeThickness = 0;
                }

                series.ItemsSource = items;
            }
        }

        protected abstract TType3 OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TType3[] items);


        protected abstract IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, TType>> collection);

        //protected virtual IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, TType>> collection) =>
        //   collection.Select(a => a.Value)
        //    .GroupBy(a => a)
        //    .Select(a =>
        //    {
        //        if (a == null)
        //        {

        //        }

        //        return a;
        //    })
        //    .Scan(seed: default(TType), (a, b) => CreatePoint(a, b))
        //    .Skip(1);





        //protected virtual IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, TType>> collection) =>
        //collection
        //.Select(a => a.Value)
        //.Select(a =>
        //{
        //    if (a == null)
        //    {

        //    }

        //    return a;
        //})
        //.Scan(seed: default(TType), (a, b) => CreatePoint(a, b))
        //.Skip(1)
        //.Cast<TType3>();


        //            protected override IEnumerable<ITimeStatsRangePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, ITimeStatsPoint<TKey>>> collection)
        //{
        //    var ees = collection
        //        .OrderBy(a => a.Value.Key);

        //    var se = ranges != null ? Ranges() : NoRanges();

        //    return se.ToArray();

        //    IEnumerable<ITimeStatsRangePoint<TKey>> Ranges()
        //    {
        //        return ees
        //            .GroupOn(ranges, a => a.Value.Var)
        //            .Where(a => a.Any())
        //            .Scan((default(TimeStatsRangePoint<TKey>), default(ITimeStatsPoint<TKey>)), (ac, bc) =>
        //            {
        //                var ss = bc
        //                .Select(a => a.Value)
        //                .Scan(ac.Item2, (a, b) => CreatePoint(a, b))
        //                .Cast<ITimeStatsPoint<TKey>>()
        //                .Skip(1)
        //                .ToArray();
        //                var value2 = ac.Item2.Value2;
        //                return (new TimeStatsRangePoint<TKey>(bc.Key, ss, value2, bc.FirstOrDefault().Value.Key, this.operation.HasValue ? operation.Value : Operation.Mean), ss.Last());
        //            })
        //            .Skip(1)
        //            .Select(a => a.Item1)
        //            .Cast<ITimeStatsRangePoint<TKey>>();
        //    }

        //    IEnumerable<ITimeStatsRangePoint<TKey>> NoRanges()
        //    {
        //        return base.ToDataPoints(collection)
        //            .Scan((a, b) =>
        //            {
        //                var sa = a.Value2 ?? new OnTheFlyStats.Stats();
        //                var es = new TimeStatsRangePoint<TKey>(new Range<DateTime>(b.Var, b.Var), new ITimePoint<TKey>[] { b }, sa, a.Key);
        //                sa.Update(es.Value);
        //                return es;
        //            });
        //    }
        //}

        //protected abstract TType3 Convert(TType type);

        protected abstract TType CreatePoint(TType xy0, TType xy);


        public override IDisposable Subscribe(IObserver<TType3> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(int count)
        {
            this.takeLastCount = count;
            refreshSubject.OnNext(Unit.Default);
        }

        public override void OnNext(KeyValuePair<TGroupKey, TType> item)
        {
            lock (temporaryCollection)
                temporaryCollection.Add(item);
            refreshSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<TType3[]> observer)
        {
            return pointsSubject.Subscribe(observer);
        }

        public IDisposable Subscribe(IObserver<Exception> observer)
        {
            return exceptionSubject.Subscribe(observer);
        }
    }
}
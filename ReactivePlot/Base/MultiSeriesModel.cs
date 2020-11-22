#nullable enable

using Kaos.Collections;
using MoreLinq;
using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ReactivePlot.Base
{
    public abstract class MultiSeriesModel<TKey, TVar, TType> : MultiSeriesModel<TKey, TVar, TType, TType>, IObserver<int>

        where TType : IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {
        public MultiSeriesModel(IPlotModel<TType> model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, max, min, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<TType> ToDataPoints(IEnumerable<KeyValuePair<TKey, TType>> collection) =>
            collection
            .Select(a => a.Value)
            .Select(a => { return a; })
            .Scan(seed: default(TType), (a, b) => CreatePoint(a, b))
            .Skip(1);

    }

    public abstract class MultiSeriesModel<TKey, TVar, TType, TType3> : MultiSeriesModel<TKey, TKey, TVar, TType, TType3>, IObserver<int>
        where TType3 : TType
        where TType : Model.IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {
        public MultiSeriesModel(IPlotModel<TType3> model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, max, min, comparer, scheduler: scheduler)
        {
        }
    }

    public abstract class MultiSeriesModel<TGroupKey, TKey, TVar, TType, TType3> :
        MultiSeries2BaseModel<TGroupKey, TKey, TVar, TType, TType3>,
        IObservable<TType3[]>,
        IObserver<int>,
        IObserver<IComparer<TGroupKey>>,
        IObservable<Exception>
        where TType3 : TType
        where TType : IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {

        private const string AllSeriesTitle = "All";
        protected readonly Collection<KeyValuePair<TGroupKey, TType>> temporaryCollection = new Collection<KeyValuePair<TGroupKey, TType>>();
        protected readonly Subject<TType3[]> pointsSubject = new Subject<TType3[]>();
        protected readonly Subject<Exception> exceptionSubject = new Subject<Exception>();
        protected readonly IPlotModel<TType3> plotModel;
        protected int? takeLastCount;
        private IComparer<TGroupKey>? comparer;

        public MultiSeriesModel(IPlotModel<TType3> plotModel, TVar max, TVar min, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) :
            base(plotModel, comparer, scheduler: scheduler)
        {
            this.plotModel = plotModel;
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
                    plotModel.Invalidate(true);
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
            foreach (var keyValue in (comparer != null ? dataPoints.OrderBy(a => a.Key, comparer) : dataPoints.AsEnumerable()).Index())
            {
                _ = await Task.Run(() =>
                {
                    return CreateSingle(keyValue.Value).ToArray();
                }).ContinueWith(async points =>
                {
                    plotModel.AddData(await points, keyValue.Value.Key?.ToString() ?? string.Empty, keyValue.Key);
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
                        plotModel.AddData(taskPoints, AllSeriesTitle, dataPoints.Length);

                    if (pointsSubject.HasObservers)
                    {
                        pointsSubject.OnNext(taskPoints);
                    }
                });
            }
            else
            {
                _ = plotModel.RemoveSeries(AllSeriesTitle);
            }
        }

        protected virtual IEnumerable<TType3> CreateSingle(KeyValuePair<TGroupKey, ICollection<TType>> keyValue)
        {
            return Create(Flatten(keyValue.Value, keyValue.Key));
        }

        protected virtual IEnumerable<TType3> CreateMany(IEnumerable<KeyValuePair<TGroupKey, ICollection<TType>>> keyValues)
        {
            return Create(Flatten(keyValues.SelectMany(a => a.Value).OrderBy(a => a.Var).Scan((a, b) => CreateAllPoint(a, b))));
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

        //Dictionary<string, IDisposable> disposableDictionary = new Dictionary<string, IDisposable>();

        //protected virtual void AddToSeries(TType3[] items, string title, int? index = null)
        //{
        //    lock (plotModel)
        //    {
        //        if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
        //        {
        //            series = OxyFactory.BuildWithMarker(items, title);

        //            series
        //                .ToMouseDownEvents()
        //                .Select(args => OxyMouseDownAction(args, series, items))
        //                .Subscribe(subject.OnNext);

        //            if (index.HasValue)
        //                plotModel.Series.Insert(index.Value, series);
        //            else
        //                plotModel.Series.Add(series);

        //        }
        //        if (series is LineSeries lSeries)
        //        {
        //            var count = series.ItemsSource.Count();
        //            lSeries.MarkerSize = (int)(5/ (1 + (Math.Log10(count)))) - 1;
        //            if (count > 100)
        //                lSeries.MarkerStrokeThickness = 0;
        //        }

        //        series.ItemsSource = items;
        //    }
        //}

        //protected virtual bool RemoveSeries(string title)
        //{
        //    lock (plotModel)
        //    {
        //        //if (index.HasValue)
        //        //{
        //        //    plotModel.Series.RemoveAt(index.Value);
        //        //    return;
        //        //}

        //        if ((plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
        //        {
        //            disposableDictionary.Remove(title);
        //            plotModel.Series.Remove(series);
        //            return true;
        //        }
        //        return false;
        //    }
        //}


        //protected abstract TType3 OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TType3[] items);


        protected abstract IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, TType>> collection);


        protected abstract TType CreatePoint(TType xy0, TType xy);

        protected virtual TType CreateAllPoint(TType xy0, TType xy)
        {
            return CreatePoint(xy0,xy);
        }


        //public override IDisposable Subscribe(IObserver<TType3> observer)
        //{
        //    return subject.Subscribe(observer);
        //}

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

        public void OnNext(IComparer<TGroupKey> comparer)
        {
            this.comparer = comparer;
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
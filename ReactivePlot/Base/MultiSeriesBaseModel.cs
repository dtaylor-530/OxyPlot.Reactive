#nullable enable

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
    public abstract class MultiSeriesBaseModel<TGroupKey, TKey, TVar, TType, TType3> :
    SeriesBaseModel<TGroupKey, TKey, TVar, TType, TType3>,
    IObservable<TType3[]>,
    IObserver<int>,
    IObserver<IComparer<TGroupKey>>,
    IObservable<Exception>
    where TVar : IComparable<TVar>
    {
        protected readonly Collection<KeyValuePair<TGroupKey, TType>> temporaryCollection = new Collection<KeyValuePair<TGroupKey, TType>>();
        protected readonly Subject<TType3[]> pointsSubject = new Subject<TType3[]>();
        protected readonly Subject<Exception> exceptionSubject = new Subject<Exception>();
        protected readonly IMultiPlotModel<TType3> plotModel;
        protected int? takeLastCount;
        private IComparer<TGroupKey>? comparer;

        public MultiSeriesBaseModel(IMultiPlotModel<TType3> plotModel, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) :
            base(plotModel, comparer, scheduler: scheduler)
        {
            this.plotModel = plotModel;
        }

        protected override async void Refresh(IList<Unit> units)
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

        protected virtual async Task AddAllPointsToSeries(KeyValuePair<TGroupKey, ICollection<TType>>[] dataPoints)
        {
            foreach (var keyValue in (comparer != null ? dataPoints.OrderBy(a => a.Key, comparer) : dataPoints.AsEnumerable()).Index())
            {
                _ = await Task.Run(() =>
                {
                    return CreateSingle(keyValue.Value).ToArray();
                }).ContinueWith(async points =>
                {
                    plotModel.AddSeries(await points, keyValue.Value.Key?.ToString() ?? string.Empty, keyValue.Key);
                });
            }
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

        protected virtual IEnumerable<TType3> CreateSingle(KeyValuePair<TGroupKey, ICollection<TType>> keyValue)
        {
            return Create(keyValue.Value);
        }

        protected virtual IEnumerable<TType3> Create(IEnumerable<TType> value)
        {
            return takeLastCount.HasValue ?
                                 Enumerable.TakeLast(ToDataPoints(value), takeLastCount.Value) :
                                 ToDataPoints(value);
        }

        protected virtual void PreModify()
        {
        }

        protected virtual IEnumerable<TType3> ToDataPoints(IEnumerable<TType> collection)
        {
            return
                collection
                .Scan(seed: default(TType3)!, (a, b) => CreateNewPoint(a, b))
                .Skip(1);
        }

        protected abstract TType3 CreateNewPoint(TType3 xy0, TType xy);

        protected override void Remove(ISet<TGroupKey> names)
        {
            base.Remove(names);

            _ = (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (plotModel)
                {
                    //TODO fix
                    foreach (var name in names.Select(a => a.ToString()))
                        plotModel.RemoveSeries(name);
                    refreshSubject.OnNext(Unit.Default);
                }
            });
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
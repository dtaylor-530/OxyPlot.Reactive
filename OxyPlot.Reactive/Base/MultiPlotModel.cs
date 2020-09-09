#nullable enable

using MoreLinq;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Series;
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
    public abstract class MultiPlotModel<TKey, TVar, TType> : MultiPlotModel<TKey, TVar, TType, TType>, IObserver<int>
        where TType : Model.IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {
        public MultiPlotModel(PlotModel model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, max, min, comparer, scheduler: scheduler)
        {
        }
    }

    public abstract class MultiPlotModel<TKey, TVar, TType, TType3> : MultiPlotModel2Base<TKey, TVar, TType, TType3>, IObservable<TType3[]>, IObserver<int>
        where TType : Model.IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {
        protected readonly Subject<TType3> subject = new Subject<TType3>();
        protected readonly List<KeyValuePair<TKey, TType>> list = new List<KeyValuePair<TKey, TType>>();
        private readonly Subject<TType3[]> pointsSubject = new Subject<TType3[]>();
        protected int? takeLastCount;

        public MultiPlotModel(PlotModel model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            this.Max = max;
            this.Min = min;
        }

        protected TVar Min { get; set; }
        protected TVar Max { get; set; }

        protected override sealed async void Refresh(IList<Unit> units)
        {
            await Task.Run(() =>
            {
                lock (list)
                {
                    if (list.Any())
                    {
                        AddToDataPoints(list.ToArray());
                        list.Clear();
                    }
                }
            });

            _ = (this as IMixedScheduler).ScheduleAction(async () =>
            {
                KeyValuePair<TKey, ICollection<TType>>[]? dataPoints;

                this.PreModify();

                lock (DataPoints)
                    dataPoints = DataPoints.ToArray();
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
                try
                {
                    plotModel.InvalidatePlot(true);
                }
                catch (Exception e)
                {
                }
            });
        }

        protected virtual IEnumerable<TType3> CreateSingle(KeyValuePair<TKey, ICollection<TType>> keyValue)
        {
            return Create(Flatten(keyValue.Value, keyValue.Key));
        }

        protected virtual IEnumerable<TType3> CreateMany(IEnumerable<KeyValuePair<TKey, ICollection<TType>>> keyValues)
        {
            return Create(Flatten(keyValues.SelectMany(a => a.Value)));
        }

        private IEnumerable<KeyValuePair<TKey, TType>> Flatten(IEnumerable<TType> value, TKey key = default)
        => value.ToArray().Select(c => KeyValuePair.Create(key, c));

        protected virtual IEnumerable<TType3> Create(IEnumerable<KeyValuePair<TKey, TType>> value)
        {
            return takeLastCount.HasValue ?
                                 Enumerable.TakeLast(ToDataPoints(value), takeLastCount.Value) :
                                 ToDataPoints(value);
        }

        protected override void AddToDataPoints(IEnumerable<KeyValuePair<TKey, TType>> items)
        {
            Min = CalculateMin(items);
            Max = CalculateMax(items);
            base.AddToDataPoints(items);
        }

        protected abstract TVar CalculateMin(IEnumerable<KeyValuePair<TKey, TType>> items);

        protected abstract TVar CalculateMax(IEnumerable<KeyValuePair<TKey, TType>> items);

        protected virtual void PreModify()
        {
        }

        protected virtual void AddToSeries(TType3[] items, string title)
        {
            lock (plotModel)
            {
                if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
                {
                    series = OxyFactory.Build(items.Cast<DataPoint>(), title);

                    series
                        .ToMouseDownEvents()
                        .Select(args => OxyMouseDownAction(args, series, items))
                        .Subscribe(subject.OnNext);

                    plotModel.Series.Add(series);
                }

                series.ItemsSource = items;
            }
        }

        protected abstract TType3 OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TType3[] items);

        protected abstract IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TKey, TType>> collection);

        public override IDisposable Subscribe(IObserver<TType3> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(int count)
        {
            this.takeLastCount = count;
            refreshSubject.OnNext(Unit.Default);
        }

        public override void OnNext(KeyValuePair<TKey, TType> item)
        {
            lock (list)
                list.Add(item);
            refreshSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<TType3[]> observer)
        {
            return pointsSubject.Subscribe(observer);
        }
    }
}
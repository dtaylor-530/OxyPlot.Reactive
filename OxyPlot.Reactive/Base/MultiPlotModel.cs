#nullable enable
using Kaos.Collections;
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
using System.Threading;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{

    public abstract class MultiPlotModel<TKey, TVar, TType> : MultiPlotModel<TKey, TVar, TType, TType>, IObservable<TType>, IObserver<int> where TType : Model.IDoublePoint<TKey, TVar>
    {
        public MultiPlotModel(PlotModel model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, max, min, comparer, scheduler: scheduler)
        {
        }
    }



    public abstract class MultiPlotModel<TKey, TVar, TType, TType3> : MultiPlotModel<TKey, TVar>, IObservable<TType>, IObserver<int> where TType : Model.IDoublePoint<TKey, TVar>
    {
        protected readonly Subject<TType> subject = new Subject<TType>();
        protected readonly List<KeyValuePair<TKey, KeyValuePair<TVar, double>>> list = new List<KeyValuePair<TKey, KeyValuePair<TVar, double>>>();
        protected int? count;


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
                KeyValuePair<TKey, ICollection<KeyValuePair<TVar, double>>>[]? dataPoints;
              

                this.PreModify();

                lock (DataPoints)
                    dataPoints = DataPoints.ToArray();
                foreach (var keyValue in dataPoints)
                {
                    _ = await Task.Run(() =>
                    {
                        return CreateSingle(keyValue).ToArray();

                    }).ContinueWith(async points => AddToSeries(await points, keyValue.Key?.ToString() ?? string.Empty));
                }

                if (showAll)
                {
                    _ = await Task.Run(() =>
                    {
                        lock (DataPoints)
                            return CreateMany(DataPoints).ToArray();

                    }).ContinueWith(async points => AddToSeries(await points, "All"));
                }
                try
                {
                    plotModel.InvalidatePlot(true);
                }
                catch(Exception e)
                {

                }
            });

        }

        protected virtual IEnumerable<TType3> CreateSingle(KeyValuePair<TKey, ICollection<KeyValuePair<TVar, double>>> keyValue)
        {
            return Create(Flatten(keyValue.Value, keyValue.Key));
        }

        protected virtual IEnumerable<TType3> CreateMany(IEnumerable<KeyValuePair<TKey, ICollection<KeyValuePair<TVar, double>>>> keyValues)
        {
            return Create(Flatten(keyValues.SelectMany(a => a.Value)));
        }

        IEnumerable<KeyValuePair<TKey, KeyValuePair<TVar, double>>> Flatten(IEnumerable<KeyValuePair<TVar, double>> value, TKey key = default)
        => value.ToArray().Select(c => KeyValuePair.Create(key, c));

        protected virtual IEnumerable<TType3> Create(IEnumerable<KeyValuePair<TKey, KeyValuePair<TVar, double>>> value)
        {
            return count.HasValue ?
                              Enumerable.TakeLast(ToDataPoints(value), count.Value) :
                              ToDataPoints(value);
        }

        protected virtual void AddToDataPoints(ICollection<KeyValuePair<TKey, KeyValuePair<TVar, double>>> items)
        {
            try
            {
                Min = CalculateMin(items);
                Max = CalculateMax(items);

                lock (DataPoints)
                {
                    foreach (var item in items)
                    {
                        if (!DataPoints.ContainsKey(item.Key))
                            DataPoints[item.Key] = new RankedMap<TVar, double>();
                        DataPoints[item.Key].Add(item.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected abstract TVar CalculateMin(ICollection<KeyValuePair<TKey, KeyValuePair<TVar, double>>> items);
        protected abstract TVar CalculateMax(ICollection<KeyValuePair<TKey, KeyValuePair<TVar, double>>> items);


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


        protected abstract TType OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TType3[] items);


        protected override void AddToDataPoints(KeyValuePair<TKey, KeyValuePair<TVar, double>> item)
        {
            lock (list)
                list.Add(item);
        }

        protected abstract IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<TVar, double>>> collection);



        public IDisposable Subscribe(IObserver<TType> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(int count)
        {
            this.count = count;
            refreshSubject.OnNext(Unit.Default);
        }

        public abstract void OnNext(TType item);

    }



    public abstract class MultiPlotModel<T, TR> : MultiPlotModelBase<T, KeyValuePair<TR, double>>
    {
        public MultiPlotModel(PlotModel plotModel, IEqualityComparer<T>? comparer = null, int refreshRate = 100, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
        }

        public MultiPlotModel(PlotModel plotModel, IEqualityComparer<T>? comparer = null, int refreshRate = 100, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
        }

    }
}




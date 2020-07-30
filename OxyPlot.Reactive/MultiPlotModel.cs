#nullable enable
using Kaos.Collections;
using MoreLinq;
using OxyPlot.Reactive.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using e = System.Linq.Enumerable;

namespace OxyPlot.Reactive
{

    public abstract class MultiPlotModel<TKey, TVar, TType> : MultiPlotModel<TKey, TVar, TType, TType>, IObservable<TType>, IObserver<int> where TType : Model.IDoublePoint<TKey, TVar>
    {
        public MultiPlotModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

    }



    public abstract class MultiPlotModel<TKey, TVar, TType, TType3> : MultiPlotModel<TKey, TVar>, IObservable<TType>, IObserver<int> where TType : Model.IDoublePoint<TKey, TVar>
    {
        protected readonly Subject<TType> subject = new Subject<TType>();
        protected readonly List<KeyValuePair<TKey, KeyValuePair<TVar, double>>> list = new List<KeyValuePair<TKey, KeyValuePair<TVar, double>>>();
        protected int? count;


        public MultiPlotModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected TVar min { get; set; }
        protected TVar max { get; set; }

        protected override async void Refresh(IList<Unit> units)
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
                lock (DataPoints)
                    dataPoints = DataPoints.ToArray();

                this.Modify();

                foreach (var keyValue in dataPoints)
                {
                    _ = await Task.Run(() =>
                    {
                        lock (DataPoints)
                        {
                            return Create(Flatten(keyValue.Key, keyValue.Value));
                        }
                    }).ContinueWith(async points => AddToSeries(await points, keyValue.Key.ToString()));
                }

                if (showAll)
                {
                    _ = await Task.Run(() =>
                    {
                        lock (DataPoints)
                        {
                            return Create(Flatten(default, DataPoints.SelectMany(a => a.Value)));
                        }
                    }).ContinueWith(async points => AddToSeries(await points, "All"));
                }
                plotModel.InvalidatePlot(true);
            });

            IEnumerable<KeyValuePair<TKey, KeyValuePair<TVar, double>>> Flatten(TKey key, IEnumerable<KeyValuePair<TVar, double>> value)
            => value.ToArray().Select(c => KeyValuePair.Create(key, c));
        }

        protected virtual TType3[] Create(IEnumerable<KeyValuePair<TKey, KeyValuePair<TVar, double>>> value)
        {
            return count.HasValue ?
                              Enumerable.TakeLast(ToDataPoints(value), count.Value).ToArray() :
                              ToDataPoints(value).ToArray();
        }

        protected virtual void AddToDataPoints(ICollection<KeyValuePair<TKey, KeyValuePair<TVar, double>>> items)
        {
            try
            {
                min = CalculateMin(items);
                max = CalculateMax(items);

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


        protected virtual void Modify()
        {
        }

        protected abstract void AddToSeries(TType3[] items, string title);

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


    public abstract class MultiPlotModelBase<TKey, TValue> : DataPointsModel<TKey, TValue>, IObserver<KeyValuePair<TKey, TValue>>, IObserver<bool>, IMixedScheduler
    {
        //private readonly IEqualityComparer<TKey>? comparer;
        private readonly SynchronizationContext? context;
        public IScheduler? scheduler;
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly PlotModel plotModel;
        protected readonly object lck = new object();
        protected bool showAll;


        public MultiPlotModelBase(PlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = default) : this(plotModel, comparer, refreshRate)
        {
            this.scheduler = scheduler ?? Scheduler.CurrentThread;
        }

        public MultiPlotModelBase(PlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = default) : this(plotModel, comparer, refreshRate)
        {
            this.context = context ?? SynchronizationContext.Current;
        }

        private MultiPlotModelBase(PlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100) : base(comparer)
        {
            this.plotModel = plotModel ?? throw new ArgumentNullException("PlotModel is null");
            ModifyPlotModel();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(refreshRate)).Where(e.Any).Subscribe(Refresh);

            if (plotModel?.PlotView?.ActualController is IController plotController)
                CongfigureBindings(plotController);
        }

        protected virtual void ModifyPlotModel() { }

        public void OnNext(KeyValuePair<TKey, TValue> item)
        {
            this.AddToDataPoints(item);
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(bool showAll)
        {
            this.showAll = showAll;
        }

        protected virtual double Combine(double x0, double x1) => x1;

        public void Reset()
        {
            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (DataPoints)
                    DataPoints.Clear();

                lock (plotModel)
                {
                    plotModel.Series.Clear();
                    plotModel.InvalidatePlot(true);
                }
            });

        }

        public void Remove(ISet<TKey> names)
        {
            (this as IMixedScheduler).ScheduleAction(() =>

            {
                lock (DataPoints)
                    RemoveFromDataPoints(names);
                lock (plotModel)
                {
                    Sdf(s => names.Select(a => a.ToString()).Contains(s.Title));
                    plotModel.InvalidatePlot(true);
                }
            });

            void Sdf(Predicate<Series.Series> pred)
            {

                while (plotModel.Series.Any(pred.Invoke))
                    plotModel.Series.Remove(plotModel.Series.First(pred.Invoke));
            }
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiPlotModelBase<TKey, TValue>)}", error);


        protected abstract void Refresh(IList<Unit> units);


        protected void CongfigureBindings(IController pc)
        {
            pc.UnbindMouseDown(OxyMouseButton.Left);
            //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control);
            //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift);

            pc.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                         (view, controller, args) =>
                            controller.AddMouseManipulator(view, new TrackerManipulator1(view), args)));
        }


        IScheduler? IMixedScheduler.Scheduler => scheduler;

        SynchronizationContext? IMixedScheduler.Context => context;
    }


    public class DataPointsModel<TKey, TValue>
    {
        protected readonly Dictionary<TKey, ICollection<TValue>> DataPoints;
        protected readonly IEqualityComparer<TKey>? comparer;

        public DataPointsModel(IEqualityComparer<TKey>? comparer = null)
        {
            DataPoints = GetDataPoints();
            this.comparer = comparer;
        }

        protected virtual void AddToDataPoints(KeyValuePair<TKey, TValue> item)
        {
            var newdp = item.Value;
            lock (DataPoints)
            {
                if (!DataPoints.ContainsKey(item.Key))
                    DataPoints[item.Key] = new List<TValue>();
                DataPoints[item.Key].Add(newdp);
            }
        }

        protected virtual void RemoveFromDataPoints(IEnumerable<TKey> keys)
        {
            lock (DataPoints)
            {
                foreach (var key in keys)
                    if (DataPoints.ContainsKey(key))
                        DataPoints.Remove(key);
            }
        }

        protected virtual Dictionary<TKey, ICollection<TValue>> GetDataPoints()
        {
            return comparer == default ?
                  new Dictionary<TKey, ICollection<TValue>>() :
                new Dictionary<TKey, ICollection<TValue>>(comparer);
        }
    }
}




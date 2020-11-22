#nullable enable

using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using e = System.Linq.Enumerable;

namespace ReactivePlot.Base
{

    public abstract class MultiSeries2BaseModel<TGroupKey, TKey, TR, TRS, TRS2> : MultiSeriesBaseModel<TGroupKey, TKey, TRS>
    {
        public MultiSeries2BaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 1000, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
        }

        public MultiSeries2BaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 1000, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
        }
    }

    public abstract class MultiSeries2BaseModel<TKey, TR, TRS, TRS2> : MultiSeriesBaseModel<TKey, TKey, TRS>, IObservable<TRS2>
    {
        protected readonly IPlotModel<TRS> plotModel;

        public MultiSeries2BaseModel(IPlotModel<TRS> plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 1000, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
            this.plotModel = plotModel;
        }

        public MultiSeries2BaseModel(IPlotModel<TRS> plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 1000, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
            this.plotModel = plotModel;
        }

        public abstract IDisposable Subscribe(IObserver<TRS2> observer);
    }

    public abstract class MultiSeries2BaseModel<TKey, TR> : MultiSeriesBaseModel<TKey, TKey, KeyValuePair<TR, double>>
    {
        public MultiSeries2BaseModel(IPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
        }

        public MultiSeries2BaseModel(IPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
        }
    }

    public abstract class MultiSeriesBaseModel<TGroupKey, TKey, TValue> : DataPointsModel<TGroupKey, TValue>, IObserver<KeyValuePair<TGroupKey, TValue>>, IObserver<bool>, IMixedScheduler
    {
        private readonly SynchronizationContext? context;
        public IScheduler? scheduler;
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        private readonly IPlotModel plotModel;
        protected readonly object lck = new object();
        protected bool showAll;

        public MultiSeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = default) : this(plotModel, comparer, refreshRate)
        {
            this.scheduler = scheduler ?? Scheduler.CurrentThread;
        }

        public MultiSeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = default) : this(plotModel, comparer, refreshRate)
        {
            this.context = context ?? SynchronizationContext.Current;
        }

        private MultiSeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 100) : base(comparer)
        {
            this.plotModel = plotModel ?? throw new ArgumentNullException("PlotModel is null");
         
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(refreshRate)).Where(e.Any).Subscribe(Refresh);
        }


        public virtual void OnNext(KeyValuePair<TGroupKey, TValue> item)
        {
            this.AddToDataPoints(new[] { item });
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(bool showAll)
        {
            this.showAll = showAll;
            refreshSubject.OnNext(Unit.Default);
        }

        public void Reset()
        {
            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (DataPoints)
                    DataPoints.Clear();

                lock (plotModel)
                {
                    plotModel.ClearSeries();
                    plotModel.Invalidate(true);
                }
            });
        }

        public void Remove(ISet<TGroupKey> names)
        {
            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (DataPoints)
                    RemoveFromDataPoints(names);
                lock (plotModel)
                {
                    foreach (var name in names.Select(a => a.ToString()))
                        plotModel.RemoveSeries(name);
                    plotModel.Invalidate(true);
                }
            });

        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiSeriesBaseModel<TGroupKey, TKey, TValue>)}", error);

        protected abstract void Refresh(IList<Unit> units);

        IScheduler? IMixedScheduler.Scheduler => scheduler;

        SynchronizationContext? IMixedScheduler.Context => context;
    }
}
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
    public abstract class SeriesBaseModel<TGroupKey, TKey, TR, TRS, TRS2> :
        SeriesBaseModel<TGroupKey, TKey, TRS> 
    {
        public SeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 1000, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
        }

        public SeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 1000, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
        }
    }

    public abstract class SeriesBaseModel<TKey, TR, TRS, TRS2> : 
        BaseModel<TKey, TRS>, IObservable<TRS2>
    {
        protected readonly IMultiPlotModel<TRS> plotModel;

        public SeriesBaseModel(IMultiPlotModel<TRS> plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 1000, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
            this.plotModel = plotModel;
        }

        public SeriesBaseModel(IMultiPlotModel<TRS> plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 1000, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
            this.plotModel = plotModel;
        }

        public abstract IDisposable Subscribe(IObserver<TRS2> observer);
    }



    public abstract class SeriesBaseModel<TKey, TType> : BaseModel<TKey, KeyValuePair<TType, double>>
    {
        public SeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
        }

        public SeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
        }
    }



    public abstract class SeriesBaseModel<TGroupKey, TKey, TType> :
        DataPointsModel<TGroupKey, TType>,
        IObserver<TType>,
        IObserver<KeyValuePair<TGroupKey, TType>>,
        IMixedScheduler
    {

        const int RefreshRate = 100;
        private readonly SynchronizationContext? context;
        public IScheduler? scheduler;
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        private readonly IPlotModel plotModel;

        public SeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = RefreshRate, IScheduler? scheduler = default) : this(plotModel, comparer, refreshRate)
        {
            this.scheduler = scheduler ?? Scheduler.CurrentThread;
        }

        public SeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = RefreshRate, SynchronizationContext? context = default) : this(plotModel, comparer, refreshRate)
        {
            this.context = context ?? SynchronizationContext.Current;
        }

        private SeriesBaseModel(IPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = RefreshRate) : base(comparer)
        {
            this.plotModel = plotModel ?? throw new ArgumentNullException("PlotModel is null");
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(refreshRate)).Where(e.Any).Subscribe(Refresh);
        }


        public virtual void OnNext(TType item)
        {
            this.AddToDataPoints(new[] { KeyValuePair.Create(GetKey(item), item) });
            refreshSubject.OnNext(Unit.Default);
        }

        public virtual void OnNext(KeyValuePair<TGroupKey, TType> item)
        {
            this.AddToDataPoints(new[] { item });
            refreshSubject.OnNext(Unit.Default);
        }

        protected abstract TGroupKey GetKey(TType item);


        protected override void Reset()
        {
            base.Reset();

            _ = (this as IMixedScheduler).ScheduleAction(() =>
              {
                  lock (plotModel)
                  {
                      plotModel.Clear();
                      refreshSubject.OnNext(Unit.Default);
                  }
              });
        }


        protected abstract void Refresh(IList<Unit> units);

        IScheduler? IMixedScheduler.Scheduler => scheduler;

        SynchronizationContext? IMixedScheduler.Context => context;
    }


    public abstract class BaseModel<TKey, TType> :
        DataPointsModel<TKey, TType>,
        IObserver<KeyValuePair<TKey, TType>>,
        IMixedScheduler
    {
        private readonly SynchronizationContext? context;
        public IScheduler? scheduler;
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        private readonly IPlotModel plotModel;

        public BaseModel(IPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = default) : this(plotModel, comparer, refreshRate)
        {
            this.scheduler = scheduler ?? Scheduler.CurrentThread;
        }

        public BaseModel(IPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = default) : this(plotModel, comparer, refreshRate)
        {
            this.context = context ?? SynchronizationContext.Current;
        }

        private BaseModel(IPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100) : base(comparer)
        {
            this.plotModel = plotModel ?? throw new ArgumentNullException("PlotModel is null");
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(refreshRate)).Where(e.Any).Subscribe(Refresh);
        }

        public virtual void OnNext(KeyValuePair<TKey, TType> item)
        {
            this.AddToDataPoints(new[] { item });
            refreshSubject.OnNext(Unit.Default);
        }
        protected override void Reset()
        {
            base.Reset();

            _ = (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (plotModel)
                {
                    plotModel.Clear();
                    refreshSubject.OnNext(Unit.Default);
                }
            });
        }

        //protected override void Remove(ISet<TKey> names)
        //{
        //    base.Remove(names);

        //    _ = (this as IMixedScheduler).ScheduleAction(() =>
        //    {
        //        lock (plotModel)
        //        {
        //            //TODO fix
        //            foreach (var name in names.Select(a => a.ToString()))
        //                plotModel.RemoveSeries(name);
        //            plotModel.Invalidate(true);
        //        }
        //    });
        //}

        //public void OnCompleted()
        //{
        //    //throw new NotImplementedException();
        //}

       // public void OnError(Exception error) => throw new Exception($"Error in {nameof(BaseModel<TKey, TType>)}", error);

        protected abstract void Refresh(IList<Unit> units);

        IScheduler? IMixedScheduler.Scheduler => scheduler;

        SynchronizationContext? IMixedScheduler.Context => context;
    }
}
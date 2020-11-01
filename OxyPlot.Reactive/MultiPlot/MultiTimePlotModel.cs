#nullable enable

using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace OxyPlot.Reactive.Multi
{
    public abstract class MultiTimePlotModel<TGroupKey, TKey, TModelType> : MultiTimePlotModel<TGroupKey, TKey, TModelType,  ITimeGroupPoint<TGroupKey, TKey>, ITimePoint<TKey>>
        where TModelType : TimeModel<TGroupKey, TKey, ITimePoint<TKey>, ITimePoint<TKey>>
    {
        public MultiTimePlotModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) : base(comparer, scheduler, synchronizationContext)
        {
        }
    }

    public abstract class MultiTimePlotModel<TGroupKey, TKey, TModelType, TGroupPoint, TPointIn> : MultiTimePlotModel<TGroupKey, TKey, TModelType, TGroupPoint, TPointIn, TPointIn>
        where TModelType : TimeModel<TGroupKey, TKey, TPointIn, TPointIn>
        where TGroupPoint : ITimeGroupPoint<TGroupKey, TKey>, TPointIn
        where TPointIn : ITimePoint<TKey>
    {
        public MultiTimePlotModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) : base(comparer, scheduler, synchronizationContext)
        {
        }
    }


    public abstract class MultiTimePlotModel<TGroupKey, TKey, TModelType, TGroupPoint, TPointIn, TPointOut> : IObserver<KeyValuePair<TGroupKey, TGroupPoint>>, IObservable<KeyValuePair<TGroupKey, PlotModel>>, IMixedScheduler
        where TModelType : TimeModel<TGroupKey, TKey, TPointIn, TPointOut>
        where TGroupPoint : ITimeGroupPoint<TGroupKey, TKey>, TPointIn
        where TPointIn:  ITimePoint<TKey>
        where TPointOut : TPointIn
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly Dictionary<TGroupKey, TModelType> Models = new Dictionary<TGroupKey, TModelType>();
        protected readonly ReplaySubject<KeyValuePair<TGroupKey, PlotModel>> PlotModelChanges = new ReplaySubject<KeyValuePair<TGroupKey, PlotModel>>();
        protected readonly IEqualityComparer<TGroupKey>? comparer;


        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

        public MultiTimePlotModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null)
        {
            this.comparer = comparer;
            this.Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.CurrentThread;
            this.Context = synchronizationContext ?? SynchronizationContext.Current;
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiTimePlotModel<TGroupKey, TKey, TModelType, TGroupPoint, TPointIn, TPointOut>)}", error);

        public void OnNext(KeyValuePair<TGroupKey, TGroupPoint> value)
        {
            AddToDataPoints(value);
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext2(KeyValuePair<TGroupKey, TGroupPoint> value)
        {
            AddToDataPoints(value);
            refreshSubject.OnNext(Unit.Default);
        }

        protected virtual void AddToDataPoints(KeyValuePair<TGroupKey, TGroupPoint> item)
        {
            lock (Models)
            {
                (this as IMixedScheduler).ScheduleAction(() =>
                {
                    if (!Models.ContainsKey(item.Key))
                    {
                        var plotModel = new PlotModel();
                        Models[item.Key] = CreateModel(plotModel);
                        PlotModelChanges.OnNext(Create(item.Key, plotModel));
                    }
                    Models[item.Key].OnNext(Create(item.Value.GroupKey, (TPointIn)item.Value));
                });
            }
        }

        protected abstract TModelType CreateModel(PlotModel plotModel);

        public IDisposable Subscribe(IObserver<KeyValuePair<TGroupKey, PlotModel>> observer) => PlotModelChanges.Subscribe(observer.OnNext);
    }


    public abstract class MultiTimePlotBModel<TGroupKey, TKey, TModelType, TPoint, TPointIn, TPointOut> : IObserver<KeyValuePair<TGroupKey, TPoint>>, IObservable<KeyValuePair<TGroupKey, PlotModel>>, IMixedScheduler
        where TModelType : TimeModel<TGroupKey, TKey, TPointIn, TPointOut>
        where TPoint : ITimeGroupPoint<TGroupKey, TKey>, TPointIn
        where TPointIn : ITimePoint<TKey>
        where TPointOut : TPointIn
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly Dictionary<TGroupKey, TModelType> Models = new Dictionary<TGroupKey, TModelType>();
        protected readonly ReplaySubject<KeyValuePair<TGroupKey, PlotModel>> PlotModelChanges = new ReplaySubject<KeyValuePair<TGroupKey, PlotModel>>();
        protected readonly IEqualityComparer<TGroupKey>? comparer;


        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

        public MultiTimePlotBModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null)
        {
            this.comparer = comparer;
            this.Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.CurrentThread;
            this.Context = synchronizationContext ?? SynchronizationContext.Current;
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiTimePlotModel<TGroupKey, TKey, TModelType, TPoint, TPointIn, TPointOut>)}", error);

        public void OnNext(KeyValuePair<TGroupKey, TPoint> value)
        {
            AddToDataPoints(value);
            refreshSubject.OnNext(Unit.Default);
        }

        protected virtual void AddToDataPoints(KeyValuePair<TGroupKey, TPoint> item)
        {
            lock (Models)
            {
                (this as IMixedScheduler).ScheduleAction(() =>
                {
                    if (!Models.ContainsKey(item.Key))
                    {
                        var plotModel = new PlotModel();
                        Models[item.Key] = CreateModel(plotModel);
                        PlotModelChanges.OnNext(Create(item.Key, plotModel));
                    }
                    Models[item.Key].OnNext(Create(item.Value.GroupKey, (TPointIn)item.Value));
                });
            }
        }

        protected abstract TModelType CreateModel(PlotModel plotModel);

        public IDisposable Subscribe(IObserver<KeyValuePair<TGroupKey, PlotModel>> observer) => PlotModelChanges.Subscribe(observer.OnNext);
    }
}
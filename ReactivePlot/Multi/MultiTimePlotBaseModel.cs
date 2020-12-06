#nullable enable

using ReactivePlot;
using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace ReactivePlot.Multi
{
    public interface IPlotGroupModel<TGroupKey, TKey> : IMultiPlotModel<ITimeGroupPoint<TGroupKey, TKey>> { }

    public abstract class MultiTimePlotModel<TGroupKey, TKey, TModelType, TPlotModelIn, TPlotModelOut> : 
        MultiTimePlotModel<TGroupKey, TKey, TModelType, ITimeGroupPoint<TGroupKey, TKey>, ITimePoint<TKey>, TPlotModelIn, TPlotModelOut> 
        where TModelType : TimeMinMaxModel<TGroupKey, TKey, ITimePoint<TKey>, ITimePoint<TKey>>
        where TPlotModelOut : IPlotModel
          where TPlotModelIn : TPlotModelOut
    {
        public MultiTimePlotModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) : base(comparer, scheduler, synchronizationContext)
        {
        }
    }

    public abstract class MultiTimePlotModel<TGroupKey, TKey, TModelType, TGroupPoint, TPointIn, TPlotModelIn, TPlotModelOut> :
        MultiTimePlotBaseModel<TGroupKey, TKey, TModelType, TGroupPoint, TPointIn, TPointIn, TPlotModelIn, TPlotModelOut> 
        where TModelType : TimeMinMaxModel<TGroupKey, TKey, TPointIn, TPointIn>
        where TGroupPoint : ITimeGroupPoint<TGroupKey, TKey>, TPointIn
        where TPointIn : ITimePoint<TKey>
        where TPlotModelOut : IPlotModel
          where TPlotModelIn : TPlotModelOut
    {
        public MultiTimePlotModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) : base(comparer, scheduler, synchronizationContext)
        {
        }
    }


    public abstract class MultiTimePlotBaseModel<TGroupKey, TKey, TModelType, TGroupPoint, TPointIn, TPointOut, TPlotModelIn, TPlotModelOut> :
        IObserver<KeyValuePair<TGroupKey, TGroupPoint>>,
        IObservable<KeyValuePair<TGroupKey, TPlotModelOut>>,
        IMixedScheduler
        where TModelType : TimeMinMaxModel<TGroupKey, TKey, TPointIn, TPointOut>
        where TGroupPoint : ITimeGroupPoint<TGroupKey, TKey>, TPointIn
        where TPointIn : ITimePoint<TKey>
        where TPointOut : TPointIn
        where TPlotModelOut : IPlotModel
           where TPlotModelIn : TPlotModelOut
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly Dictionary<TGroupKey, TModelType> Models = new Dictionary<TGroupKey, TModelType>();
        protected readonly ReplaySubject<KeyValuePair<TGroupKey, TPlotModelOut>> PlotModelChanges = new ReplaySubject<KeyValuePair<TGroupKey, TPlotModelOut>>();
        protected readonly IEqualityComparer<TGroupKey>? comparer;


        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

        public MultiTimePlotBaseModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null)
        {
            this.comparer = comparer;
            Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.CurrentThread;
            Context = synchronizationContext ?? SynchronizationContext.Current;
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiTimePlotBaseModel<TGroupKey, TKey, TModelType, TGroupPoint, TPointIn, TPointOut, TPlotModelIn, TPlotModelOut>)}", error);

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
                        var plotModel = CreatePlotModel();
                        Models[item.Key] = CreateModel(plotModel);
                        PlotModelChanges.OnNext(Create(item.Key, (TPlotModelOut)plotModel));
                    }
                    Models[item.Key].OnNext(Create(item.Value.GroupKey, (TPointIn)item.Value));
                });
            }
        }

        protected abstract TModelType CreateModel(TPlotModelIn plotModel);

        protected abstract TPlotModelIn CreatePlotModel();

        public IDisposable Subscribe(IObserver<KeyValuePair<TGroupKey, TPlotModelOut>> observer) => PlotModelChanges.Subscribe(observer.OnNext);
    }


    public abstract class MultiTimePlotBModel<TGroupKey, TKey, TModelType, TGroupPoint, TPlotModelIn, TPlotModelOut> :
        IObserver<KeyValuePair<TGroupKey, TGroupPoint>>,
        IObservable<KeyValuePair<TGroupKey, TPlotModelOut>>, 
        IMixedScheduler
        where TModelType : TimeMinMaxModel<TGroupKey, TKey, TGroupPoint, TGroupPoint>
        where TGroupPoint : ITimeGroupPoint<TGroupKey, TKey>
        where TPlotModelOut : IPlotModel
        where TPlotModelIn : IMultiPlotModel<ITimeStatsGroupPoint<string, double>>, TPlotModelOut
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly Dictionary<TGroupKey, TModelType> Models = new Dictionary<TGroupKey, TModelType>();
        protected readonly ReplaySubject<KeyValuePair<TGroupKey, TPlotModelOut>> PlotModelChanges = new ReplaySubject<KeyValuePair<TGroupKey, TPlotModelOut>>();
        protected readonly IEqualityComparer<TGroupKey>? comparer;


        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

        public MultiTimePlotBModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null)
        {
            this.comparer = comparer;
            Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.CurrentThread;
            Context = synchronizationContext ?? SynchronizationContext.Current;
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiTimePlotBModel<TGroupKey, TKey, TModelType, TGroupPoint, TPlotModelIn, TPlotModelOut>)}", error);

        public void OnNext(KeyValuePair<TGroupKey, TGroupPoint> value)
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
                        var plotModel = CreatePlotModel();
                        Models[item.Key] = CreateModel(plotModel);
                        PlotModelChanges.OnNext(Create(item.Key, (TPlotModelOut)plotModel));
                    }
                    Models[item.Key].OnNext(Create(item.Value.GroupKey, item.Value));
                });
            }
        }

        protected abstract TModelType CreateModel(IMultiPlotModel<ITimeStatsGroupPoint<string, double>> plotModel);

        protected abstract TPlotModelIn CreatePlotModel();

        public IDisposable Subscribe(IObserver<KeyValuePair<TGroupKey, TPlotModelOut>> observer) => PlotModelChanges.Subscribe(observer.OnNext);
    }
}
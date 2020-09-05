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
    public class MultiTimePlotModel<TGroupKey, TKey> : MultiTimePlotModel<TGroupKey, TKey, TimeModel<TKey>>
    {
        public MultiTimePlotModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) : base(comparer, scheduler, synchronizationContext)
        {
        }

        protected override TimeModel<TKey> CreateModel(PlotModel plotModel)
        {
            return new TimeModel<TKey>(plotModel, this.comparer, this.Scheduler);
        }
    }


    public abstract class MultiTimePlotModel<TGroupKey, TKey, TType> : MultiTimePlotModel<TGroupKey, TKey, TType, ITimePoint<TKey>> where TType : TimeModel<TKey>
    {
        public MultiTimePlotModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) : base(comparer, scheduler, synchronizationContext)
        {
        }
    }


    public abstract class MultiTimePlotModel<TGroupKey, TKey, TType, TType2> : MultiTimePlotModel<TGroupKey, TKey, TType, TType2, TType2> where TType2 : ITimePoint<TKey> where TType : TimeModel<TKey, TType2, TType2>
    {
        public MultiTimePlotModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null):base(comparer, scheduler, synchronizationContext)
        {

        }

    }

    public abstract class MultiTimePlotModel<TGroupKey, TKey, TType, TType2, TType3> : IObserver<KeyValuePair<TGroupKey, TType2>>, IObservable<KeyValuePair<TGroupKey, PlotModel>>, IMixedScheduler
   where TType : TimeModel<TKey, TType2, TType3> where TType2 : ITimePoint<TKey> where TType3 : TType2
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly Dictionary<TGroupKey, TType> Models = new Dictionary<TGroupKey, TType>();
        protected readonly ReplaySubject<KeyValuePair<TGroupKey, PlotModel>> PlotModelChanges = new ReplaySubject<KeyValuePair<TGroupKey, PlotModel>>();
        protected readonly IEqualityComparer<TKey>? comparer;

        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

        public MultiTimePlotModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null)
        {
            this.comparer = comparer;
            this.Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.CurrentThread;
            this.Context = synchronizationContext ?? SynchronizationContext.Current;
        }
        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiTimePlotModel<TGroupKey, TKey>)}", error);

        public void OnNext(KeyValuePair<TGroupKey, TType2> value)
        {
            AddToDataPoints(value);
            refreshSubject.OnNext(Unit.Default);
        }

        protected virtual void AddToDataPoints(KeyValuePair<TGroupKey, TType2> item)
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
                    Models[item.Key].OnNext(item.Value);
                });
            }
        }

        protected abstract TType CreateModel(PlotModel plotModel);


        public IDisposable Subscribe(IObserver<KeyValuePair<TGroupKey, PlotModel>> observer) => PlotModelChanges.Subscribe(observer.OnNext);

    }
}

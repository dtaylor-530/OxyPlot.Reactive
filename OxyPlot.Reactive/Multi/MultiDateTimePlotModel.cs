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


    public class MultiDateTimePlotModel<TGroupKey, TKey> : MultiDateTimePlotModel<TGroupKey, TKey, MultiDateTimeModel<TKey>>
    {
        public MultiDateTimePlotModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null):base(comparer, scheduler, synchronizationContext)
        {
        }

        protected override MultiDateTimeModel<TKey> CreateModel(PlotModel plotModel)
        {
            return new MultiDateTimeModel<TKey>(plotModel, this.comparer, this.Scheduler);
        }
    }


    public abstract class MultiDateTimePlotModel<TGroupKey, TKey, TType> : MultiDateTimePlotModel<TGroupKey, TKey, TType, IDateTimePoint<TKey>> where TType : MultiDateTimeModel<TKey>
    {
        public MultiDateTimePlotModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) : base(comparer, scheduler, synchronizationContext)
        {
        }
    }


    public abstract class MultiDateTimePlotModel<TGroupKey, TKey, TType, TType2> : IObserver<KeyValuePair<TGroupKey, IDateTimePoint<TKey>>>, IObservable<KeyValuePair<TGroupKey, PlotModel>>, IMixedScheduler where TType : MultiDateTimeModel<TKey, TType2> where TType2: IDateTimePoint<TKey>
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly Dictionary<TGroupKey, TType> Models = new Dictionary<TGroupKey, TType>();
        protected readonly ReplaySubject<KeyValuePair<TGroupKey, PlotModel>> PlotModelChanges = new ReplaySubject<KeyValuePair<TGroupKey, PlotModel>>();
        protected readonly IEqualityComparer<TKey>? comparer;

        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

        public MultiDateTimePlotModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null)
        {
            this.comparer = comparer;
            this.Scheduler = scheduler;
            this.Context = synchronizationContext;
        }
        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiDateTimePlotModel<TGroupKey, TKey>)}", error);

        public void OnNext(KeyValuePair<TGroupKey, IDateTimePoint<TKey>> value)
        {
            AddToDataPoints(value);
            refreshSubject.OnNext(Unit.Default);
        }

        protected virtual void AddToDataPoints(KeyValuePair<TGroupKey, IDateTimePoint<TKey>> item)
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

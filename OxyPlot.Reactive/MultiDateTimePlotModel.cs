#nullable enable

using NestedDictionaryLib;
using OxyPlot.Reactive.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;

namespace OxyPlot.Reactive
{
    public class MultiDateTimePlotModel<TGroupKey, TKey> : IObserver<KeyValuePair<TGroupKey, KeyValuePair<TKey, KeyValuePair<DateTime, double>>>>, IObservable<PlotModel>, IMixedScheduler
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        //private readonly IEqualityComparer<TKey>? comparer;
        //private readonly IScheduler? scheduler;
        protected Dictionary<TGroupKey, MultiDateTimeModel<TKey>> Models = new Dictionary<TGroupKey, MultiDateTimeModel<TKey>>();
        protected Subject<PlotModel> PlotModelChanges = new Subject<PlotModel>();
        private IEqualityComparer<TKey>? comparer;

        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

        public MultiDateTimePlotModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null)
        {
            this.comparer = comparer;
            this.Scheduler = scheduler;
            this.Context = synchronizationContext;
        }

        protected virtual double Combine(double x0, double x1) => x1;

        //public void Reset() => RemoveByPredicate(a => true);

        //public void Remove(ISet<string> names) => RemoveByPredicate(s => names.Contains(s.Title));

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new NotImplementedException($"Error in {nameof(MultiDateTimePlotModel<TGroupKey, TKey>)}");

        public void OnNext(KeyValuePair<TGroupKey, KeyValuePair<TKey, KeyValuePair<DateTime, double>>> value)
        {
            AddToDataPoints(value);
            refreshSubject.OnNext(Unit.Default);
        }

        protected virtual void AddToDataPoints(KeyValuePair<TGroupKey, KeyValuePair<TKey, KeyValuePair<DateTime, double>>> item)
        {
            lock (Models)
            {
                (this as IMixedScheduler).ScheduleAction(() =>
                {
                    if (!Models.ContainsKey(item.Key))
                    {
                        var plotModel = new PlotModel();
                        Models[item.Key] = new MultiDateTimeModel<TKey>(plotModel, this.comparer, this.Scheduler);
                        PlotModelChanges.OnNext(plotModel);
                    }
                    Models[item.Key].OnNext(item.Value);
                });
            }
        }

        public IDisposable Subscribe(IObserver<PlotModel> observer)
        {
            return PlotModelChanges.Subscribe(observer.OnNext);
        }
    }
}

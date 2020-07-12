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

namespace OxyPlot.Reactive
{

    public class MultiDateTimePlotAccumulatedModel<TGroupKey, TKey> : MultiDateTimePlotModel<TGroupKey, TKey>
    {
        private ErrorBarModel errorBarModel;

        public MultiDateTimePlotAccumulatedModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = new PlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(TGroupKey), plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<TGroupKey, IDateTimeKeyPoint<TKey>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key?.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected override MultiDateTimeModel<TKey> CreateModel(PlotModel plotModel)
        {
            return new MultiDateTimeAccumulatedModel<TKey>(plotModel, this.comparer, this.Scheduler);
        }

    }

    public class MultiDateTimePlotModel<TGroupKey, TKey> : IObserver<KeyValuePair<TGroupKey, IDateTimeKeyPoint<TKey>>>, IObservable<KeyValuePair<TGroupKey, PlotModel>>, IMixedScheduler
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly Dictionary<TGroupKey, MultiDateTimeModel<TKey>> Models = new Dictionary<TGroupKey, MultiDateTimeModel<TKey>>();
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

        protected virtual double Combine(double x0, double x1) => x1;

        //public void Reset() => RemoveByPredicate(a => true);

        //public void Remove(ISet<string> names) => RemoveByPredicate(s => names.Contains(s.Title));

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiDateTimePlotModel<TGroupKey, TKey>)}", error);

        public void OnNext(KeyValuePair<TGroupKey, IDateTimeKeyPoint<TKey>> value)
        {
            AddToDataPoints(value);
            refreshSubject.OnNext(Unit.Default);
        }

        protected virtual void AddToDataPoints(KeyValuePair<TGroupKey, IDateTimeKeyPoint<TKey>> item)
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

        protected virtual MultiDateTimeModel<TKey> CreateModel(PlotModel plotModel)
        {
            return new MultiDateTimeModel<TKey>(plotModel, this.comparer, this.Scheduler);
        }

        public IDisposable Subscribe(IObserver<KeyValuePair<TGroupKey, PlotModel>> observer) => PlotModelChanges.Subscribe(observer.OnNext);

    }
}

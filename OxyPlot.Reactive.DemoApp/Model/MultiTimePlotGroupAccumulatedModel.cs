#nullable enable

using OxyPlot;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Multi;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace OxyPlot.Reactive.DemoApp.Model
{
    public class MultiTimePlotGroupAccumulatedModel<TGroupKey, TKey> :
        MultiTimePlotModel<TGroupKey, TKey,
            TimeAccumulatedGroupModel<TGroupKey, TKey>, 
            ITimeGroupPoint<TGroupKey, TKey>, 
            ITimePoint<TKey>, 
            ITimeRangePoint<TKey>>, 
        IObserver<Operation>, IObserver<TimeSpan>
    {
        private readonly ReplaySubject<TimeSpan> timeSpan = new ReplaySubject<TimeSpan>();
        private readonly ReplaySubject<Operation> operation = new ReplaySubject<Operation>();
        private readonly ErrorBarModel errorBarModel;

        public MultiTimePlotGroupAccumulatedModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = new PlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(TGroupKey), plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<TGroupKey, ITimeGroupPoint<TGroupKey, TKey>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key?.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected override TimeAccumulatedGroupModel<TGroupKey, TKey> CreateModel(PlotModel plotModel)
        {
            var model = new TimeAccumulatedGroupModel<TGroupKey, TKey>(plotModel, comparer, Scheduler);
            operation.Subscribe(model);
            timeSpan.Subscribe(model);
            return model;
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan.OnNext(value);
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(Operation value)
        {
            operation.OnNext(value);
            refreshSubject.OnNext(Unit.Default);
        }
    }
}
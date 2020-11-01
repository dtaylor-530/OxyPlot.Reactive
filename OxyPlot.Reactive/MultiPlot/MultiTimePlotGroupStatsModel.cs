#nullable enable

using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Model.Enum;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace OxyPlot.Reactive.Multi
{

    public interface ITimeStatsGroupPoint<TGroupKey, TKey> :  ITimeGroupPoint<TGroupKey, TKey>, ITimeStatsPoint<TKey>
    {
    }

    public class MultiTimePlotGroupStatsModel<TGroupKey, TKey> : 
        MultiTimePlotModel<TGroupKey,
            TKey, 
            TimeOnTheFlyStatsGroupModel<TGroupKey, TKey>,
            ITimeStatsGroupPoint<TGroupKey, TKey>,
            ITimeStatsPoint<TKey>, 
            ITimeStatsRangePoint<TKey>>,
        IObserver<Operation>, 
        IObserver<TimeSpan>,
        IObserver<RollingOperation>
    {
        private readonly ReplaySubject<TimeSpan> timeSpan = new ReplaySubject<TimeSpan>(1);
        private readonly ReplaySubject<Operation> operation = new ReplaySubject<Operation>(1);
        private readonly ReplaySubject<RollingOperation> rollingOperation = new ReplaySubject<RollingOperation>(1);
        private readonly ErrorBarModel errorBarModel;

        public MultiTimePlotGroupStatsModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = new PlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(TGroupKey), plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<TGroupKey, ITimeStatsGroupPoint<TGroupKey, TKey>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key?.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected override TimeOnTheFlyStatsGroupModel<TGroupKey, TKey> CreateModel(PlotModel plotModel)
        {
            var model = new TimeOnTheFlyStatsGroupModel<TGroupKey, TKey>(plotModel, this.comparer, this.Scheduler);
            operation.Subscribe(model);
            timeSpan.Subscribe(model);
            rollingOperation.Subscribe(model);
            return model;
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan.OnNext(value);
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(Operation value)
        {
            this.operation.OnNext(value);
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(RollingOperation value)
        {
            this.rollingOperation.OnNext(value);
            refreshSubject.OnNext(Unit.Default);
        }
    }
}
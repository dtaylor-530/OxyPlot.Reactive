using ReactivePlot;
using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using ReactivePlot.Time;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace ReactivePlot.Multi
{



    public abstract class MultiTimePlotGroupStatsModel<TGroupKey, TKey, TPlotModelIn, TPlotModelOut, TPlotModelError> :
        MultiTimePlotBaseModel<TGroupKey,
            TKey,
            TimeOnTheFlyStatsGroupModel<TGroupKey, TKey>,
            ITimeStatsGroupPoint<TGroupKey, TKey>,
            ITimeStatsPoint<TKey>,
            ITimeStatsRangePoint<TKey>,
            TPlotModelIn,
            TPlotModelOut>,
        IObserver<Operation>,
        IObserver<TimeSpan>,
        IObserver<RollingOperation>
        where TPlotModelOut : IPlotModel
        where TPlotModelIn : TPlotModelOut, IMultiPlotModel<ITimeStatsRangePoint<TKey>>
        where TPlotModelError: TPlotModelOut, IMultiPlotModel<(string, ErrorPoint)>
    {
        private readonly ReplaySubject<TimeSpan> timeSpan = new ReplaySubject<TimeSpan>(1);
        private readonly ReplaySubject<Operation> operation = new ReplaySubject<Operation>(1);
        private readonly ReplaySubject<RollingOperation> rollingOperation = new ReplaySubject<RollingOperation>(1);
        private readonly ErrorBarModel errorBarModel;

        public MultiTimePlotGroupStatsModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = CreateErrorPlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(TGroupKey), (TPlotModelOut)plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<TGroupKey, ITimeStatsGroupPoint<TGroupKey, TKey>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key?.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected abstract TPlotModelError CreateErrorPlotModel();

        protected override TimeOnTheFlyStatsGroupModel<TGroupKey, TKey> CreateModel(TPlotModelIn plotModel)
        {
            var model = new TimeOnTheFlyStatsGroupModel<TGroupKey, TKey>(plotModel, comparer, Scheduler);
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
            operation.OnNext(value);
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(RollingOperation value)
        {
            rollingOperation.OnNext(value);
            refreshSubject.OnNext(Unit.Default);
        }
    }
}
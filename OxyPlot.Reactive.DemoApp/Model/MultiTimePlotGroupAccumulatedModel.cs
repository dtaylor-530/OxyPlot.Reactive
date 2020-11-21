#nullable enable


using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.DemoApp.Model;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using ReactivePlot.Multi;
using ReactivePlot.OxyPlot;
using ReactivePlot.OxyPlot.Common;
using ReactivePlot.OxyPlot.PlotModel;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace OxyPlot.Reactive.DemoApp.Model
{

    public class MultiTimePlotGroupAccumulatedModel<TGroupKey, TKey> : MultiTimePlotGroupAccumulatedBaseModel<TGroupKey, TKey>

    {
        public MultiTimePlotGroupAccumulatedModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
           base(comparer, scheduler, synchronizationContext)
        {

        }

    }


    public class MultiTimePlotGroupAccumulatedBaseModel<TGroupKey, TKey> :
        MultiTimePlotBaseModel<TGroupKey, 
            TKey,
            TimeAccumulatedGroupModel<TGroupKey, TKey>,
            ITimeGroupPoint<TGroupKey, TKey>,
            ITimePoint<TKey>,
            ITimeRangePoint<TKey>,
            OxyTimePlotModel<TKey, ITimeRangePoint<TKey>>,
            IOxyPlotModel>,
        IObserver<Operation>, IObserver<TimeSpan>
    {
        private readonly ReplaySubject<TimeSpan> timeSpan = new ReplaySubject<TimeSpan>();
        private readonly ReplaySubject<Operation> operation = new ReplaySubject<Operation>();
        private readonly ErrorBarModel errorBarModel;

        public MultiTimePlotGroupAccumulatedBaseModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = CreateErrorPlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(TGroupKey), (IOxyPlotModel)plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<TGroupKey, ITimeGroupPoint<TGroupKey, TKey>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key?.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected ErrorBarPlotModel CreateErrorPlotModel()
        {
            return new ErrorBarPlotModel(new PlotModel());
        }

        protected override TimeAccumulatedGroupModel<TGroupKey, TKey> CreateModel(OxyTimePlotModel<TKey, ITimeRangePoint<TKey>> plotModel)
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

        protected override OxyTimePlotModel<TKey, ITimeRangePoint<TKey>> CreatePlotModel()
        {
            return new OxyTimePlotModel<TKey, ITimeRangePoint<TKey>>(new PlotModel());
        }
    }
}
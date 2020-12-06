#nullable enable

using ReactivePlot;
using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace ReactivePlot.Multi
{

    public abstract class MultiTimePlotAccumulatedModel<TKey, TPlotModelIn, TPlotModelOut, TPlotModelError> :
        MultiTimePlotAccumulatedModel<TKey, TKey, TPlotModelIn, TPlotModelOut, TPlotModelError> 
        where TPlotModelOut : IPlotModel
        where TPlotModelIn : TPlotModelOut, IMultiPlotModel<ITimeGroupPoint<TKey, TKey>>
        where TPlotModelError : IMultiPlotModel<(string, ErrorPoint)>, TPlotModelOut
    {
        public MultiTimePlotAccumulatedModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
          base(comparer, scheduler, synchronizationContext)
        {

        }
    }

    public abstract class MultiTimePlotAccumulatedModel<TGroupKey, TKey, TPlotModelIn,  TPlotModelOut, TPlotModelError> :
        MultiTimePlotBaseModel<
            TGroupKey,
            TKey,
            TimeAccumulatedAModel<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>,
            ITimeGroupPoint<TGroupKey, TKey>,
            ITimeGroupPoint<TGroupKey, TKey>,
            TPlotModelIn,
            TPlotModelOut>
        where TPlotModelIn : TPlotModelOut, IMultiPlotModel<ITimeGroupPoint<TGroupKey, TKey>>
        where TPlotModelOut : IPlotModel
        where TPlotModelError : IMultiPlotModel<(string, ErrorPoint)>, TPlotModelOut
    {
        private ErrorBarModel errorBarModel;

        public MultiTimePlotAccumulatedModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = CreateErrorPlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(TGroupKey), (TPlotModelOut)plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<TGroupKey, ITimeGroupPoint<TGroupKey, TKey>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key?.ToString() ?? "faadsd", item.Value.Value)));
            }
        }


        protected abstract TPlotModelError CreateErrorPlotModel();

        protected override TimeAccumulatedAModel<TGroupKey, TKey> CreateModel(TPlotModelIn plotModel)
        {
            return new TimeAccumulatedAModel<TGroupKey, TKey>(plotModel, CreatePoint, comparer, Scheduler);
        }

        protected virtual ITimeGroupPoint<TGroupKey, TKey> CreatePoint(ITimeGroupPoint<TGroupKey, TKey> xy0, ITimeGroupPoint<TGroupKey, TKey> xy)
        {
            return new TimeGroupPoint<TGroupKey, TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key, xy.GroupKey);
        }
    }


    public class TimeAccumulatedAModel<TGroupKey, TKey> : TimeGroupKeyModel<TGroupKey, TKey>
    {
        private readonly Func<ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>> func;

        public TimeAccumulatedAModel(IMultiPlotModel<ITimeGroupPoint<TGroupKey, TKey>> model, Func<ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>> func, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null)
            : base(model, comparer, scheduler)
        {
            this.func = func;
        }

        protected override ITimeGroupPoint<TGroupKey, TKey> CreateNewPoint(ITimeGroupPoint<TGroupKey, TKey> xy0, ITimeGroupPoint<TGroupKey, TKey> xy)
        {
            return func(xy0, xy);
        }




        //protected override ITimeGroupPoint<TGroupKey, TKey> CreatePoint(ITimeGroupPoint<TGroupKey, TKey> xy0, ITimeGroupPoint<TGroupKey, TKey> xy)
        //{

        //}
    }
}
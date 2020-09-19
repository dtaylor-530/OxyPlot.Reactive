#nullable enable

using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace OxyPlot.Reactive.Multi
{

    public class MultiTimePlotAccumulatedModel<TKey> : MultiTimePlotAccumulatedModel<TKey, TKey>
    {
        public MultiTimePlotAccumulatedModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
          base(comparer, scheduler, synchronizationContext)
        {

        }
    }

    public class MultiTimePlotAccumulatedModel<TGroupKey, TKey> : MultiTimePlotModel<TGroupKey, TKey, TimeAccumulatedAModel<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>>
    {
        private ErrorBarModel errorBarModel;

        public MultiTimePlotAccumulatedModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
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

        protected override TimeAccumulatedAModel<TGroupKey, TKey> CreateModel(PlotModel plotModel)
        {
            return new TimeAccumulatedAModel<TGroupKey, TKey>(plotModel, CreatePoint, this.comparer, this.Scheduler);
        }

        protected virtual ITimeGroupPoint<TGroupKey, TKey> CreatePoint(ITimeGroupPoint<TGroupKey, TKey> xy0, ITimeGroupPoint<TGroupKey, TKey> xy)
        {
            return new TimeGroupPoint<TGroupKey, TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key, xy.GroupKey);
        }
    }


    public class TimeAccumulatedAModel<TGroupKey, TKey> : TimeGroupKeyModel<TGroupKey, TKey>
    {
        private readonly Func<ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>> func;

        public TimeAccumulatedAModel(PlotModel model, Func<ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>> func, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
            this.func = func;
        }

        protected override ITimeGroupPoint<TGroupKey, TKey> CreatePoint(ITimeGroupPoint<TGroupKey, TKey> xy0, ITimeGroupPoint<TGroupKey, TKey> xy)
        {
            return func(xy0, xy);
        }
    }
}
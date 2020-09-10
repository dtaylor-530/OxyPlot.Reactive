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
    public class MultiTimePlotAccumulatedModel<TGroupKey, TKey> : MultiTimePlotModel<TGroupKey, TKey, TimeAccumulatedModel<TKey>, ITimePoint<TKey>>
    {
        private ErrorBarModel errorBarModel;

        public MultiTimePlotAccumulatedModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = new PlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(TGroupKey), plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<TGroupKey, ITimePoint<TKey>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key?.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected override TimeAccumulatedModel<TKey> CreateModel(PlotModel plotModel)
        {
            return new TimeAccumulatedModel(plotModel, CreatePoint, this.comparer, this.Scheduler);
        }

        protected virtual ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key);
        }

        internal class TimeAccumulatedModel : TimeAccumulatedModel<TKey>
        {
            private readonly Func<ITimePoint<TKey>, ITimePoint<TKey>, ITimePoint<TKey>> func;

            public TimeAccumulatedModel(PlotModel model, Func<ITimePoint<TKey>, ITimePoint<TKey>, ITimePoint<TKey>> func, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
            {
                this.func = func;
            }

            protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
            {
                return func(xy0, xy);
            }
        }
    }
}
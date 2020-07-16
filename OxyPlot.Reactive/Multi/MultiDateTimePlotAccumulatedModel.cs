#nullable enable

using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace OxyPlot.Reactive.Multi
{
    public class MultiDateTimePlotAccumulatedModel<TGroupKey, TKey> : MultiDateTimePlotModel<TGroupKey, TKey, MultiDateTimeAccumulatedModel<TKey>, IDateTimePoint<TKey>>
    {
        private ErrorBarModel errorBarModel;

        public MultiDateTimePlotAccumulatedModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = new PlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(TGroupKey), plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<TGroupKey, IDateTimePoint<TKey>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key?.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected override MultiDateTimeAccumulatedModel<TKey> CreateModel(PlotModel plotModel)
        {
            return new MultiDateTimeAccumulatedModel<TKey>(plotModel, this.comparer, this.Scheduler);
        }
    }
}

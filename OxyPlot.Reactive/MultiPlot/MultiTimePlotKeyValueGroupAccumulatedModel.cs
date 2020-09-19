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
    public class MultiTimePlotKeyValueGroupAccumulatedModel : MultiTimePlotModel<string, double, TimeKeyValueGroupAccumulatedModel>
    {
        private ErrorBarModel errorBarModel;

        public MultiTimePlotKeyValueGroupAccumulatedModel(IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = new PlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(string), plotModel));
        }

        protected override void AddToDataPoints(KeyValuePair<string, ITimeGroupPoint<string, double>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                //_ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.Key.ToString() ?? "faadsd", item.Value.Value)));
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.GroupKey.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected override TimeKeyValueGroupAccumulatedModel CreateModel(PlotModel plotModel)
        {
            return new TimeAccumulatedModel(plotModel, CreatePoint, this.comparer, this.Scheduler);
        }

        protected virtual ITimePoint<double> CreatePoint(ITimePoint<double> xy0, ITimePoint<double> xy)
        {
            return new TimePoint<double>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key);
        }

        internal class TimeAccumulatedModel : TimeKeyValueGroupAccumulatedModel
        {
            private readonly Func<ITimePoint<double>, ITimePoint<double>, ITimePoint<double>> func;

            public TimeAccumulatedModel(PlotModel model, Func<ITimePoint<double>, ITimePoint<double>, ITimePoint<double>> func, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
            {
                this.func = func;
                this.OnNext(2d);

            }

            protected override ITimePoint<double> CreatePoint(ITimePoint<double> xy0, ITimePoint<double> xy)
            {
                return func(xy0, xy);
            }

       


        }
    }
}
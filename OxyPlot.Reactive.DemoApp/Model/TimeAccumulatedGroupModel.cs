#nullable enable

using OxyPlot;
using ReactivePlot.Model;
using ReactivePlot.OxyPlot.PlotModel;
using ReactivePlot.Time;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace ReactivePlot.DemoApp.Model
{
    public class TimeAccumulatedGroupModel<TGroupKey, TKey> : TimeGroupModel<TGroupKey, TKey>
    {
        public TimeAccumulatedGroupModel(PlotModel model, IScheduler? scheduler = null) : 
            base(new OxyTimePlotModel<TKey, ITimeRangePoint<TKey>>(model), scheduler: scheduler)
        {
        }


        public TimeAccumulatedGroupModel(IMultiPlotModel<ITimeRangePoint<TKey>> model, IScheduler? scheduler = null) : base(model, scheduler: scheduler)
        {
        }

        public TimeAccumulatedGroupModel(IMultiPlotModel<ITimeRangePoint<TKey>> model, IEqualityComparer<TGroupKey>? comparer, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key);
        }
    }
}
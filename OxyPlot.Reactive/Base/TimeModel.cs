#nullable enable

using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using OxyPlot.Series;
using MoreLinq;
using OxyPlot.Reactive.Model;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{
    public class TimeModel<TKey> : TimeModel<TKey, ITimePoint<TKey>>
    {
        public TimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
    }

    public abstract class TimeModel<TKey, TType> : TimeModel<TKey, TType, TType> where TType : ITimePoint<TKey>
    {
        public TimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }
    }


    public abstract class TimeModel<TKey, TType, TType3> : MultiPlotModel<TKey, DateTime, TType, TType3> where TType : ITimePoint<TKey> where TType3 : TType
    {
        public TimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, DateTime.MinValue, DateTime.MaxValue, comparer, scheduler: scheduler)
        {
        }

        protected override void ModifyPlotModel()
        {
            plotModel.Axes.Add(new DateTimeAxis());
        }

        protected override TType3 OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TType3[] items)
        {
            var time = DateTimeAxis.ToDateTime(series.InverseTransform(e.Position).X);
            var point = items.MinBy(a => Math.Abs((a.Var - time).Ticks)).First();
            return point;
        }

        protected override DateTime CalculateMin(IEnumerable<KeyValuePair<TKey, TType>> items)
        {
            return items.Any() ? new DateTime(Math.Min(items.Min(a => a.Value.Var.Ticks), Min.Ticks)) : Min;
        }

        protected override DateTime CalculateMax(IEnumerable<KeyValuePair<TKey, TType>> items)
        {
            return items.Any() ? new DateTime(Math.Max(items.Max(a => a.Value.Var.Ticks), Max.Ticks)) : Max;
        }


        protected override IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TKey, TType>> collection)            =>
                collection
                .Scan(new TimePoint<TKey>(), (xy0, xy) => new TimePoint<TKey>(xy.Value.Var, Combine(xy0.Value, xy.Value.Value), xy.Key))
                .Cast<TType3>()
                .Skip(1);
    }
}

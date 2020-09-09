#nullable enable

using MoreLinq;
using OxyPlot.Reactive.Model;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace OxyPlot.Reactive
{
    public class CartesianModel<TKey> : CartesianModel<TKey, IDoublePoint<TKey>>
    {
        public CartesianModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
    }

    public abstract class CartesianModel<TKey, TType> : CartesianModel<TKey, TType, TType> where TType : IDoublePoint<TKey>
    {
        public CartesianModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }
    }

    public abstract class CartesianModel<TKey, TType, TType3> : MultiPlotModel<TKey, double, TType, TType3> where TType : IDoublePoint<TKey> where TType3 : TType
    {
        public CartesianModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, double.MinValue, double.MaxValue, comparer, scheduler: scheduler)
        {
        }

        protected override TType3 OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TType3[] items)
        {
            var x = series.InverseTransform(e.Position).X;
            var point = items.MinBy(a => Math.Abs(a.Var - x)).First();
            return point;
        }

        protected override double CalculateMax(IEnumerable<KeyValuePair<TKey, TType>> items)
        {
            return items.Any() ? Math.Max(items.Max(a => a.Value.Var), Max) : Max;
        }

        protected override double CalculateMin(IEnumerable<KeyValuePair<TKey, TType>> items)
        {
            return items.Any() ? Math.Min(items.Min(a => a.Value.Var), Min) : Min;
        }

        protected override IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TKey, TType>> collection)
            => collection
            .Scan(new DoublePoint<TKey>(), (xy0, xy) => new DoublePoint<TKey>(xy.Value.Var, Combine(xy0.Value, xy.Value.Value), xy.Key))
            .Cast<TType3>()
            .Skip(1);
    }
}
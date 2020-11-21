using MoreLinq;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ReactivePlot.Base
{
    public class CartesianModel<TKey> : CartesianModel<TKey, IDoublePoint<TKey>>
    {
        public CartesianModel(IPlotModel<IDoublePoint<TKey>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
    }

    public class CartesianModel<TKey, TType> : CartesianModel<TKey, TType, TType> where TType : IDoublePoint<TKey>
    {
        public CartesianModel(IPlotModel<TType> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }

        protected override TType CreatePoint(TType xy0, TType xy)
        {
            return (TType)((IDoublePoint<TKey>)new DoublePoint<TKey>(xy.Var, xy.Value, xy.Key));
        }

        protected override IEnumerable<TType> ToDataPoints(IEnumerable<KeyValuePair<TKey, TType>> collection) =>
            collection
            .Select(a => a.Value)
            .Select(a => { return a; })
            .Scan(seed: default(TType), (a, b) => CreatePoint(a, b))
            .Skip(1);
    }

    public abstract class CartesianModel<TKey, TType, TType3> : MultiSeriesModel<TKey, double, TType, TType3> where TType : IDoublePoint<TKey> where TType3 : TType
    {
        public CartesianModel(IPlotModel<TType3> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, double.MinValue, double.MaxValue, comparer, scheduler: scheduler)
        {
        }
        protected override double CalculateMax(IEnumerable<KeyValuePair<TKey, TType>> items)
        {
            return items.Any() ? Math.Max(items.Max(a => a.Value.Var), Max) : Max;
        }

        protected override double CalculateMin(IEnumerable<KeyValuePair<TKey, TType>> items)
        {
            return items.Any() ? Math.Min(items.Min(a => a.Value.Var), Min) : Min;
        }
    }

}
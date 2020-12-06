using MoreLinq;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ReactivePlot.Base
{

    public class CartesianModel : CartesianModel<string, IDoublePoint>
    {
        public CartesianModel(IMultiPlotModel<IDoublePoint> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IDoublePoint CreateNewPoint(IDoublePoint xy0, IDoublePoint xy)
        {
            return new DoublePoint(xy.Var, xy.Value, xy.Key);
        }
    }

    public class CartesianModel<TKey> : CartesianModel<TKey, IDoublePoint<TKey>>
    {
        public CartesianModel(IMultiPlotModel<IDoublePoint<TKey>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
    }

    public class CartesianModel<TKey, TType> : CartesianModel<TKey, TType, TType> where TType : IDoublePoint<TKey>
    {
        public CartesianModel(IMultiPlotModel<TType> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }

        protected override TType CreatePoint(TType xy0, TType xy)
        {
            return CreateNewPoint(xy0, xy);
        }

        protected override TType CreateNewPoint(TType xy0, TType xy)
        {
            return (TType)((IDoublePoint<TKey>)new DoublePoint<TKey>(xy.Var, xy.Value, xy.Key));
        }

        protected override TKey GetKey(TType item)
        {
            return item.Key;
        }
    }

    public abstract class CartesianModel<TKey, TType, TType3> : MultiSeriesMinMaxModel<TKey, TKey, double, TType, TType3> 
        where TType : IVar<double>
    {
        public CartesianModel(IMultiPlotModel<TType3> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, double.MinValue, double.MaxValue, comparer, scheduler: scheduler)
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

        protected override IEnumerable<TType3> ToDataPoints(IEnumerable<TType> collection) =>
            collection
            .Scan(seed: default(TType3)!, (a, b) => CreateNewPoint(a, b))
            .Skip(1);
    }
}
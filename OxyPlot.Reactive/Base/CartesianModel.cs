﻿#nullable enable

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

        public override void OnNext(TType item)
        {
            AddToDataPoints(KeyValuePair.Create(item.Key, KeyValuePair.Create(item.Var, item.Value)));
            refreshSubject.OnNext(Unit.Default);
        }

        protected override TType3 OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TType3[] items)
        {
            var x = series.InverseTransform(e.Position).X;
            var point = items.MinBy(a => Math.Abs(a.Var - x)).First();
            return point;
        }

        protected override double CalculateMax(ICollection<KeyValuePair<TKey, KeyValuePair<double, double>>> items)
        {
            return items.Any() ? Math.Max(items.Max(a => a.Value.Key), Max) : Max;
        }

        protected override double CalculateMin(ICollection<KeyValuePair<TKey, KeyValuePair<double, double>>> items)
        {
            return items.Any() ? Math.Min(items.Min(a => a.Value.Key), Min) : Min;
        }


        protected override IEnumerable<TType3> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<double, double>>> collection)
            => collection
            .Scan(new DoublePoint<TKey>(), (xy0, xy) => new DoublePoint<TKey>(xy.Value.Key, Combine(xy0.Value, xy.Value.Value), xy.Key))
            .Cast<TType3>()
            .Skip(1);
    }
}
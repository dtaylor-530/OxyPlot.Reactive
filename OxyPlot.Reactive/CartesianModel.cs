#nullable enable

using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using OxyPlot.Series;
using MoreLinq;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{

    public class CartesianModel<TKey> : CartesianModel<TKey, IPoint<TKey>>
    {
        public CartesianModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }


        protected override IEnumerable<IPoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<double, double>>> collection)
            => collection
            .Scan(new Point<TKey>(), (xy0, xy) => new Point<TKey>(xy.Value.Key, Combine(xy0.Value, xy.Value.Value), xy.Key))
            .Cast<IPoint<TKey>>()
            .Skip(1);

    }


    public abstract class CartesianModel<TKey, TType> : MultiPlotModel<TKey, double, TType, TType> where TType : IPoint<TKey>
    {

        public CartesianModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            min = double.MaxValue;
            max = double.MinValue;
        }



        public override void OnNext(TType item)
        {
            AddToDataPoints(KeyValuePair.Create(item.Key, KeyValuePair.Create(item.Var, item.Value)));
            refreshSubject.OnNext(Unit.Default);
        }


        protected override void AddToSeries(TType[] items, string title)
        {
            lock (plotModel)
            {
                if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
                {
                    series = OxyFactory.Build(items.Cast<DataPoint>(), title);

                    series.ToMouseDownEvents().Subscribe(e =>
                    {
                        var x = series.InverseTransform(e.Position).X;
                        var point = items.MinBy(a => Math.Abs((a.Var - x))).First();
                        subject.OnNext(point);
                    });

                    plotModel.Series.Add(series);
                }

                series.ItemsSource = items;
            }
        }

        protected override double CalculateMax(ICollection<KeyValuePair<TKey, KeyValuePair<double, double>>> items)
        {
            return items.Any() ? Math.Min(items.Min(a => a.Value.Key), min) : min;
        }

        protected override double CalculateMin(ICollection<KeyValuePair<TKey, KeyValuePair<double, double>>> items)
        {
            return items.Any() ? Math.Max(items.Max(a => a.Value.Key), max) : max;
        }

    }
}

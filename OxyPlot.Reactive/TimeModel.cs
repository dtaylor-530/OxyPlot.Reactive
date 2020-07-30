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


    public class TimeModel<TKey> : TimeModel<TKey, ITimePoint<TKey>>
    {
        public TimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<ITimePoint<TKey>> ToDataPoints(IEnumerable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> collection)
            => collection
            .Scan(new TimePoint<TKey>(), (xy0, xy) => new TimePoint<TKey>(xy.Value.Key, Combine(xy0.Value, xy.Value.Value), xy.Key))
            .Cast<ITimePoint<TKey>>()
            .Skip(1);

    }


    public abstract class TimeModel<TKey, TType> : TimeModel<TKey, TType, TType> where TType : ITimePoint<TKey>
    {
        public TimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }

    }


    public abstract class TimeModel<TKey, TType, TType3> : MultiPlotModel<TKey, DateTime, TType, TType3> where TType : ITimePoint<TKey> where TType3 : TType
    {
        public TimeModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
            min = DateTime.MaxValue;
            max = DateTime.MinValue;
        }

        public override void OnNext(TType item)
        {
            AddToDataPoints(KeyValuePair.Create(item.Key, KeyValuePair.Create(item.Var, item.Value)));
            refreshSubject.OnNext(Unit.Default);
        }


        protected override void AddToSeries(TType3[] items, string title)
        {
            lock (plotModel)
            {
                if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
                {
                    series = OxyFactory.Build(items.Cast<DataPoint>(), title);

                    series.ToMouseDownEvents().Subscribe(e =>
                    {
                        var time = DateTimeAxis.ToDateTime(series.InverseTransform(e.Position).X);
                        var point = items.MinBy(a => Math.Abs((a.Var - time).Ticks)).First();
                        subject.OnNext(point);
                    });

                    plotModel.Series.Add(series);
                }

                series.ItemsSource = items;
            }
        }

        protected override void ModifyPlotModel()
        {
            plotModel.Axes.Add(new DateTimeAxis());
        }

        protected override DateTime CalculateMax(ICollection<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> items)
        {
            return items.Any() ? new DateTime(Math.Max(items.Max(a => a.Value.Key.Ticks), max.Ticks)) : max;
        }

        protected override DateTime CalculateMin(ICollection<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> items)
        {
            return items.Any() ? new DateTime(Math.Min(items.Min(a => a.Value.Key.Ticks), min.Ticks)) : min;
        }
    }
}

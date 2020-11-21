#nullable enable

using MoreLinq;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ReactivePlot.Base
{
    public class TimeModel<TKey> : TimeModel<TKey, ITimePoint<TKey>>
    {
        public TimeModel(IPlotModel<ITimePoint<TKey>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }


        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

        protected override ITimePoint<TKey> CreateAllPoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key);
        }
    }

    public abstract class TimeGroupKeyModel<TGroupKey, TKey> : TimeGroupKeyModel<TGroupKey, TKey, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>>
    {
        public TimeGroupKeyModel(IPlotModel<ITimeGroupPoint<TGroupKey, TKey>> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<ITimeGroupPoint<TGroupKey, TKey>> ToDataPoints(IEnumerable<KeyValuePair<TGroupKey, ITimeGroupPoint<TGroupKey, TKey>>> collection)
        {
            return collection
       .Select(a => a.Value)
       .Select(a => { return a; })
       .Scan(seed: default(ITimeGroupPoint<TGroupKey, TKey>), (a, b) => CreatePoint(a, b))
       .Skip(1)
       .Cast<ITimeGroupPoint<TGroupKey, TKey>>();
        }


    }

    public abstract class TimeGroupKeyModel<TGroupKey, TKey, TType, TType3> : TimeModel<TGroupKey, TKey, TType, TType3>
        where TType : ITimeGroupPoint<TGroupKey, TKey>
        where TType3 : TType
    {
        public TimeGroupKeyModel(IPlotModel<TType3> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
    }

    public abstract class TimeModel<TKey, TType> : TimeModel<TKey, TType, TType> where TType : ITimePoint<TKey>
    {
        public TimeModel(IPlotModel<TType> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }


        protected override IEnumerable<TType> ToDataPoints(IEnumerable<KeyValuePair<TKey, TType>> collection) =>
            collection
            .Select(a => a.Value)
            .Select(a => { return a; })
            .Scan(seed: default(TType), (a, b) => CreatePoint(a, b))
            .Skip(1);
    }



    public abstract class TimeModel<TKey, TType, TType3> : TimeModel<TKey, TKey, TType, TType3>
        where TType : ITimePoint<TKey>
        where TType3 : TType
    {
        public TimeModel(IPlotModel<TType3> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }
    }


    public abstract class TimeModel<TGroupKey, TKey, TType, TType3> : MultiSeriesModel<TGroupKey, TKey, DateTime, TType, TType3>
        where TType : ITimePoint<TKey> where TType3 : TType
    {
        public TimeModel(IPlotModel<TType3> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, DateTime.MinValue, DateTime.MaxValue, comparer, scheduler: scheduler)
        {
        }

        protected override DateTime CalculateMax(IEnumerable<KeyValuePair<TGroupKey, TType>> items)
        {
            return items.Any() ? new DateTime(Math.Max(items.Max(a => a.Value.Var.Ticks), Max.Ticks)) : Max;
        }

        protected override DateTime CalculateMin(IEnumerable<KeyValuePair<TGroupKey, TType>> items)
        {
            return items.Any() ? new DateTime(Math.Min(items.Min(a => a.Value.Var.Ticks), Min.Ticks)) : Min;
        }

    }
}
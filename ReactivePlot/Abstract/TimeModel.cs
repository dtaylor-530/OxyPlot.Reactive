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
        public TimeModel(IMultiPlotModel<ITimePoint<TKey>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimePoint<TKey> CreateNewPoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, xy.Value, xy.Key);
        }

        protected override ITimePoint<TKey> CreatePoint(ITimePoint<TKey> xy0, ITimePoint<TKey> xy)
        {
            return new TimePoint<TKey>(xy.Var, (xy0?.Value ?? 0) + xy.Value, xy.Key);
        }

        protected override TKey GetKey(ITimePoint<TKey> item)
        {
            return item.Key;
        }
    }

    public abstract class TimeGroupKeyModel<TGroupKey, TKey> : TimeGroupKeyModel<TGroupKey, TKey, ITimeGroupPoint<TGroupKey, TKey>, ITimeGroupPoint<TGroupKey, TKey>>
    {
        public TimeGroupKeyModel(IMultiPlotModel<ITimeGroupPoint<TGroupKey, TKey>> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

       // protected override IEnumerable<ITimeGroupPoint<TGroupKey, TKey>> ToDataPoints(IEnumerable<ITimeGroupPoint<TGroupKey, TKey>> collection)
       // {
       //     return collection
       //.Scan(seed: default(ITimeGroupPoint<TGroupKey, TKey>), (a, b) => CreateNewPoint(a, b))
       //.Skip(1)
       //.Cast<ITimeGroupPoint<TGroupKey, TKey>>();
       // }

        protected override ITimeGroupPoint<TGroupKey, TKey> CreatePoint(ITimeGroupPoint<TGroupKey, TKey> xy0, ITimeGroupPoint<TGroupKey, TKey> xy)
        {
            return CreateNewPoint(xy0, xy);
        }

    }

    public abstract class TimeGroupKeyModel<TGroupKey, TKey, TType, TType3> : TimeMinMaxModel<TGroupKey, TKey, TType, TType3>
        where TType : ITimeGroupPoint<TGroupKey, TKey>
        where TType3 : TType
    {
        public TimeGroupKeyModel(IMultiPlotModel<TType3> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }
        protected override TGroupKey GetKey(TType item)
        {
            return item.GroupKey;
        }
    }

    public abstract class TimeModel<TKey, TType> : TimeModel<TKey, TType, TType> 
       where TType : ITimePoint<TKey>
    {
        public TimeModel(IMultiPlotModel<TType> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }
    }



   // public abstract class TimeRangeModel<TKey, TType, TType3> : TimeModel<TKey, TType, TType3>
   //where TType : ITimePoint<TKey>
   // where TType3 : TType
   // {
   //     public TimeRangeModel(IMultiPlotModel<TType3> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
   //     {
   //     }

   //     protected abstract TType CreatePoint(TType xy0, TType xy);


   //     protected override TType3 CreateNewPoint(TType3 xy0, TType xy)
   //     {
   //         throw new NotImplementedException();
   //     }

   // }

    public abstract class TimeModel<TKey, TType, TType3> : TimeMinMaxModel<TKey, TKey, TType, TType3>
       where TType : IVar<DateTime>
        // where TType3 : TType
    {
        public TimeModel(IMultiPlotModel<TType3> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
        {
        }
    }


    public abstract class TimeMinMaxModel<TGroupKey, TKey, TType, TType3> : MultiSeriesMinMaxModel<TGroupKey, TKey, DateTime, TType, TType3>
            //where TType : ITimePoint<TKey>
            // where TType3 : TType
            where TType : IVar<DateTime>
    {
        public TimeMinMaxModel(IMultiPlotModel<TType3> model, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) : base(model, DateTime.MinValue, DateTime.MaxValue, comparer, scheduler: scheduler)
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
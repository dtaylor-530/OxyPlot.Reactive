#nullable enable

using Itenso.TimePeriod;
using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ReactivePlot.Time
{
    public class TimeRangeModel<TKey> : TimeModel<TKey>, IObserver<TimeSpan>
    {
        private RangeType rangeType = RangeType.None;
        private ITimeRange? dateTimeRange;
        private TimeSpan? timeSpan;

        public TimeRangeModel(IMultiPlotModel<ITimePoint<TKey>> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<ITimePoint<TKey>> Create(IEnumerable<ITimePoint<TKey>> value)
        {
            return rangeType switch
            {
                RangeType.None => ToDataPoints(value),
                RangeType.Count when takeLastCount.HasValue => Enumerable.TakeLast(ToDataPoints(value), takeLastCount.Value),
                RangeType.TimeSpan when timeSpan.HasValue => ToDataPoints(value.ToArray().Filter(timeSpan.Value, a => a.Var)),
                RangeType.DateTimeRange when dateTimeRange != null => ToDataPoints(value.Filter(dateTimeRange, a => a.Var)),
                _ => throw new ArgumentOutOfRangeException("fdssffd")
            };
        }

        public void OnNext(TimeSpan value)
        {
            timeSpan = value;
            rangeType = RangeType.TimeSpan;
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(ITimeRange value)
        {
            dateTimeRange = value;
            rangeType = RangeType.DateTimeRange;
            refreshSubject.OnNext(Unit.Default);
        }

        private enum RangeType
        {
            None,
            Count = 1,
            TimeSpan,
            DateTimeRange,
            NumberRange
        }
    }
}
#nullable enable
using MoreLinq;
using ReactivePlot.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace ReactivePlot.Base
{
    public abstract class TimeModelPointModel<TKey, R, Y> : TimeModel<TKey, R, R> where R : ITimeModelPoint<TKey, Y>
    {
        public TimeModelPointModel(IMultiPlotModel<R> model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

  

        protected override R CreateNewPoint(R xy0, R xy)
        {
            return CreatePoint(xy0, xy);
        }
    }

}

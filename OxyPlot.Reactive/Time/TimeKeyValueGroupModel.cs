#nullable enable

using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{
    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeKeyValueGroupModel : TimeKeyDoubleGroupModel<double>
    {

        public TimeKeyValueGroupModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override string CreateGroupKey(ITimePoint<double> val)
        {
            if (Power.HasValue == false)
            {
                return default(double).ToString();
            }

            int v = (int)Math.Log(val.Key, Power.Value);

            var min = Math.Pow(Power.Value, v);
            var max = Math.Pow(Power.Value, v + 1);
            return $"{min:N} - {max:N}";
        }
    }
}
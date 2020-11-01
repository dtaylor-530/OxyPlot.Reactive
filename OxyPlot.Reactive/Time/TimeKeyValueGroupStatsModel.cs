#nullable enable

using MoreLinq;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive
{
    /// <summary>
    /// Groups values by the Key, logarithmically
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeKeyValueGroupStatsModel : TimeKeyLogGroupModel<double, ITimeStatsPoint<double>>, IObserver<RollingOperation>
    {
        private RollingOperation rollingOperation;

        public TimeKeyValueGroupStatsModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        public void OnNext(RollingOperation value)
        {
            rollingOperation = value;
            refreshSubject.OnNext(Unit.Default);
        }

        protected override string CreateGroupKey(IKeyPoint<double, DateTime, double> val)
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

        protected override ITimeStatsPoint<double> CreatePoint(ITimeStatsPoint<double> xy0, ITimeStatsPoint<double> xy)
        {
            return OnTheFlyStatsHelper.Combine(xy0, xy, rollingOperation);
        }

        protected override IEnumerable<ITimeStatsPoint<double>> ToDataPoints(IEnumerable<KeyValuePair<string, ITimeStatsPoint<double>>> collection)
        {
            return
                collection
                .Select(a => a.Value)
                .Select(a => { return a; })
                .Scan(seed: default(ITimeStatsPoint<double>), (a, b) => CreatePoint(a, b))
                .Cast<ITimeStatsPoint<double>>()
                .Skip(1);
        }

    }

}
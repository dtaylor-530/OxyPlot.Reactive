using LinqStatistics;
using MoreLinq;
using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace ReactivePlot.Base
{

    public class ErrorBarModel : MultiSeriesBaseModel<string, string, double, double, (string key, ErrorPoint)>
    {

        public ErrorBarModel(IMultiPlotModel<(string key, ErrorPoint)> plotModel, SynchronizationContext? context = null, IScheduler? scheduler = null) : base(plotModel,  scheduler: scheduler)
        {
        }

        protected override (string key, ErrorPoint) CreateNewPoint((string key, ErrorPoint) xy0, double xy)
        {
            throw new NotImplementedException();
        }

        protected override string GetKey(double item)
        {
            throw new NotImplementedException();
        }

        private static KeyValuePair<string, ErrorPoint> Selector(IGrouping<string, XY<string>> grp)
        {
            var arr = grp.Select(a => a.Y).ToArray();
            // var variance = Statistics.Variance(arr);
            var sd = arr.Length > 1 ? arr.StandardDeviation() : 0;
            var mean = arr.Length > 1 ? arr.Average() : arr[0];
            // return (grp.Key, new ErrorBarItem(mean, sd) { Color = mean > 0 ? Positive : Negative });
            return KeyValuePair.Create(grp.Key, new ErrorPoint(mean, sd));
        }
    }
}

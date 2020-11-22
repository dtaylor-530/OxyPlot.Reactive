using LinqStatistics;
using ReactivePlot.Common;
using ReactivePlot.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace ReactivePlot.Base
{

    public class ErrorBarModel : SingleSeriesModel<string>
    {

        protected readonly IPlotModel<(string, ErrorPoint)> plotModel;

        public ErrorBarModel(IPlotModel<(string, ErrorPoint)> plotModel, SynchronizationContext? context = null, IScheduler? scheduler = null) : base(context: context, scheduler: scheduler)
        {
            this.plotModel = plotModel;
        }

        protected override async void Refresh(IList<Unit> units)
        {
            var points = await Task.Run(() =>
            {
                lock (lck)
                {
                    return DataPoints
                    .GroupBy(a => a.X)

                    .Select(Selector).OrderBy(a => a.errorValue.Value).ToArray();
                }
            });

            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (plotModel)
                {
                    plotModel.ClearSeries();
                    plotModel.AddData(points, "A Title");
                    plotModel.Invalidate(true);
                }
            });
        }

        //static (string key, ErrorBarItem) Selector(IGrouping<string, DataPoint<string>> grp)
        private static (string key, ErrorPoint errorValue) Selector(IGrouping<string, XY<string>> grp)
        {
            var arr = grp.Select(a => a.Y).ToArray();
            // var variance = Statistics.Variance(arr);
            var sd = arr.Length > 1 ? arr.StandardDeviation() : 0;
            var mean = arr.Length > 1 ? arr.Average() : arr[0];
            // return (grp.Key, new ErrorBarItem(mean, sd) { Color = mean > 0 ? Positive : Negative });
            return (grp.Key, new ErrorPoint(mean, sd));
        }
    }
}

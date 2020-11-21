#nullable enable

using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model.Enum;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace OxyPlot.Reactive.Multi
{
    public class MultiTimePlotKeyValueGroupStatsModel :
        MultiTimePlotModel<
            string,
            double,
            TimeLogGroupStatsKeyModel,
            ITimeStatsGroupPoint<string, double>,
            ITimeStatsPoint<double>>,
        IObserver<RollingOperation>,
        IObserver<double>
    {
        private ErrorBarModel errorBarModel;
        private ReplaySubject<RollingOperation> rollingOperationSubject = new ReplaySubject<RollingOperation>(1);
        private ReplaySubject<double> powerSubject = new ReplaySubject<double>(1);

        public MultiTimePlotKeyValueGroupStatsModel(IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var plotModel = new PlotModel();
            errorBarModel = new ErrorBarModel(plotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(string), plotModel));
        }


        protected override void AddToDataPoints(KeyValuePair<string, ITimeStatsGroupPoint<string, double>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.GroupKey.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

        protected override TimeLogGroupStatsKeyModel CreateModel(PlotModel plotModel)
        {
            var model = new TimeKeyValueGroupStatsModel(plotModel, this.comparer, this.Scheduler);
            rollingOperationSubject.Subscribe(model);
            powerSubject.Subscribe(model);
            return model;
        }


        internal class TimeKeyValueGroupStatsModel : Reactive.TimeLogGroupStatsKeyModel
        {
            public TimeKeyValueGroupStatsModel(PlotModel model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler)
            {

                //this.OnNext(2d);
            }
        }

        public void OnNext(RollingOperation value)
        {
            rollingOperationSubject.OnNext(value);
        }

        public void OnNext(double value)
        {
            powerSubject.OnNext(value);
        }
    }
}
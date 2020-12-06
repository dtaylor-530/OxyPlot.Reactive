#nullable enable

using ReactivePlot;
using ReactivePlot.Base;
using ReactivePlot.Common;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using ReactivePlot.Time;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using static System.Collections.Generic.KeyValuePair;

namespace ReactivePlot.Multi
{
    // TGroupKey, TKey, TModelType, TGroupPoint, TPointIn, TPointOut

    public abstract class MultiTimePlotKeyValueGroupStatsModel<TPlotModelIn, TPlotModelOut, TPlotModelError, TModel> :
        MultiTimePlotBModel<
            string,
            double,
            TModel,
            ITimeStatsGroupPoint<string, double>,
            TPlotModelIn, 
            TPlotModelOut>,
        IObserver<RollingOperation>,
        IObserver<double>
        where TPlotModelOut : IPlotModel
        where TPlotModelIn : IMultiPlotModel<ITimeStatsGroupPoint<string, double>>, TPlotModelOut
        where TModel : TimeLogGroupStats2KeyModel
        where TPlotModelError: IMultiPlotModel<(string, ErrorPoint)>, TPlotModelOut
    {
        private ErrorBarModel errorBarModel;
        protected ReplaySubject<RollingOperation> rollingOperationSubject = new ReplaySubject<RollingOperation>(1);
        protected ReplaySubject<double> powerSubject = new ReplaySubject<double>(1);

        public MultiTimePlotKeyValueGroupStatsModel(IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronizationContext = null) :
            base(comparer, scheduler, synchronizationContext)
        {
            var errorPlotModel = CreateErrorPlotModel();
            errorBarModel = new ErrorBarModel(errorPlotModel, synchronizationContext, scheduler);
            PlotModelChanges.OnNext(Create(default(string), (TPlotModelOut)errorPlotModel));
        }


        protected override void AddToDataPoints(KeyValuePair<string, ITimeStatsGroupPoint<string, double>> item)
        {
            base.AddToDataPoints(item);
            lock (Models)
            {
                _ = (this as IMixedScheduler).ScheduleAction(() => errorBarModel.OnNext(Create(item.Value.GroupKey.ToString() ?? "faadsd", item.Value.Value)));
            }
        }

  
        protected abstract TPlotModelError CreateErrorPlotModel();


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
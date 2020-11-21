using ReactivePlot.Model;
using ReactivePlot.Multi;
using ReactivePlot.OxyPlot.Common;
using ReactivePlot.OxyPlot.PlotModel;
using ReactivePlot.Time;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using OxyPlotModel = OxyPlot.PlotModel;

namespace ReactivePlot.OxyPlot
{
    /// <summary>
    ///  Groups all points by a common ranges.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class OxyTimeGroupOnTheFlyStatsModel<TKey> : Time.TimeGroupOnTheFlyStatsModel<TKey>
    {

        public OxyTimeGroupOnTheFlyStatsModel(OxyPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyTimePlotModel<TKey, ITimeStatsRangePoint<TKey>>(plotModel), comparer, scheduler: scheduler)
        {

        }
    }

    public class OxyTimeModel<TKey> : Base.TimeModel<TKey>
    {
        public OxyTimeModel(OxyPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyTimePlotModel<TKey>(plotModel), comparer, scheduler: scheduler)
        {

        }

    }

    public class OxyCartesianModel<TKey> : Base.CartesianModel<TKey>
    {
        public OxyCartesianModel(OxyPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyCartesianPlotModel<TKey>(plotModel), comparer, scheduler: scheduler)
        {

        }

    }

    //public class OxyTimeGroupModel<TGroupKey, TKey> : TimeGroupModel<TGroupKey, TKey>
    //{
    //    public OxyTimeGroupModel(OxyPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) :
    //        base(new TimePlotModel<TKey, ITimeGroupPoint<TGroupKey, TKey>>(plotModel), comparer, scheduler: scheduler)
    //    {

    //    }

    //}

    public class OxyTimeGroupModel<TKey> : TimeGroupModel<TKey>
    {
        public OxyTimeGroupModel(OxyPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyTimePlotModel<TKey, ITimeRangePoint<TKey>>(plotModel), comparer, scheduler: scheduler)
        {

        }

    }

    public class OxyTime2KellyModel<TKey> : Time2KellyModel<TKey>
    {
        public OxyTime2KellyModel(OxyPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyTimePlotModel<TKey, IKellyPoint<TKey>>(plotModel), comparer, scheduler: scheduler)
        {

        }

    }

    public class OxyTimeRangeModel<TKey> : TimeRangeModel<TKey>
    {
        public OxyTimeRangeModel(OxyPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyTimePlotModel<TKey>(plotModel), comparer, scheduler: scheduler)
        {

        }

    }

    public class OxyErrorBarModel : Base.ErrorBarModel
    {
        public OxyErrorBarModel(OxyPlotModel plotModel, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) :
            base(new ErrorBarPlotModel(plotModel))
        {

        }

    }

    public class OxyCartesianGroupModel<TKey> : Cartesian.CartesianGroupModel<TKey>
    {
        public OxyCartesianGroupModel(OxyPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyCartesianPlotModel<TKey, IDoubleRangePoint<TKey>>(plotModel), comparer, scheduler)
        {

        }

    }

    public class OxyTimeOnTheFlyStatsModel<TKey> : Time.TimeOnTheFlyStatsModel<TKey>
    {
        public OxyTimeOnTheFlyStatsModel(OxyPlotModel plotModel, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyTimePlotModel<TKey, ITimeModelPoint<TKey, OnTheFlyStats.Stats>>(plotModel), comparer, scheduler)
        {

        }

    }

    public class OxyTimeLogGroupValueModel<TKey> : Time.TimeLogGroupValueModel<TKey>
    {
        public OxyTimeLogGroupValueModel(OxyPlotModel plotModel, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyTimePlotModel<TKey>(plotModel), comparer, scheduler)
        {

        }

    }

    public class OxyTimeLogGroupKeyModel : Time.TimeLogGroupKeyModel
    {
        public OxyTimeLogGroupKeyModel(OxyPlotModel plotModel, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) :
            base(new OxyTimePlotModel<double>(plotModel), comparer, scheduler)
        {

        }

    }

    public class OxyMultiTimePlotGroupStatsModel<TGroupKey, TKey> : 
        MultiTimePlotGroupStatsModel<
            TGroupKey, 
            TKey, 
            OxyTimePlotModel<TKey, ITimeStatsRangePoint<TKey>>, 
            IOxyPlotModel, 
            ErrorBarPlotModel>
    {
        public OxyMultiTimePlotGroupStatsModel(IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) :
            base(comparer, scheduler)
        {

        }

        protected override ErrorBarPlotModel CreateErrorPlotModel()
        {
            return new ErrorBarPlotModel(new OxyPlotModel());
        }

        protected override OxyTimePlotModel<TKey, ITimeStatsRangePoint<TKey>> CreatePlotModel()
        {
            return new OxyTimePlotModel<TKey, ITimeStatsRangePoint<TKey>>(new OxyPlotModel());
        }
    }

    public class OxyMultiTimePlotKeyValueGroupStatsModel :
            MultiTimePlotKeyValueGroupStatsModel<
            OxyTimePlotModel<double, ITimeStatsGroupPoint<string, double>>,
            IOxyPlotModel, 
            ErrorBarPlotModel, 
            TimeLogGroupStats2KeyModel>
    {
        public OxyMultiTimePlotKeyValueGroupStatsModel(IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) :
            base(comparer, scheduler)
        {

        }


        protected override TimeLogGroupStats2KeyModel CreateModel(IPlotModel<ITimeStatsGroupPoint<string, double>> plotModel)
        {
            var model = new TimeKeyValueGroupStatsModel(plotModel, comparer, Scheduler);
            rollingOperationSubject.Subscribe(model);
            powerSubject.Subscribe(model);
            return model;
        }

        protected override ErrorBarPlotModel CreateErrorPlotModel()
        {
            return new ErrorBarPlotModel(new OxyPlotModel());
        }

        protected override OxyTimePlotModel<double, ITimeStatsGroupPoint<string, double>> CreatePlotModel()
        {
            return new OxyTimePlotModel<double, ITimeStatsGroupPoint<string, double>>(new OxyPlotModel());
        }

        internal class TimeKeyValueGroupStatsModel : TimeLogGroupStats2KeyModel
        {
            public TimeKeyValueGroupStatsModel(IPlotModel<ITimeStatsGroupPoint<string, double>> model, IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) :
                base(model, comparer, scheduler)
            {

                //this.OnNext(2d);
            }
        }
    }

    public class OxyMultiTimePlotAccumulatedModel<TKey> : 
        MultiTimePlotAccumulatedModel<
            TKey,
            OxyTimePlotModel<TKey, ITimeGroupPoint<TKey, TKey>>, 
            IOxyPlotModel, 
            ErrorBarPlotModel>
    {
        public OxyMultiTimePlotAccumulatedModel(IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) :
            base(comparer, scheduler)
        {

        }

        protected override ErrorBarPlotModel CreateErrorPlotModel()
        {
            return new ErrorBarPlotModel(new OxyPlotModel());
        }

        protected override OxyTimePlotModel<TKey, ITimeGroupPoint<TKey, TKey>> CreatePlotModel()
        {
            return new OxyTimePlotModel<TKey, ITimeGroupPoint<TKey, TKey>>(new OxyPlotModel());
        }
    }

    public class OxyMultiTimePlotAccumulatedModel : OxyMultiTimePlotAccumulatedModel<string>
    {
        public OxyMultiTimePlotAccumulatedModel(IEqualityComparer<string>? comparer = null, IScheduler? scheduler = null) :
            base(comparer, scheduler)
        {

        }
    }
}

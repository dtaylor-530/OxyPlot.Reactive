using System;
using System.Collections.Generic;
using System.Text;
using OxyPlotModel = OxyPlot.PlotModel;

namespace ReactivePlot.OxyPlot
{
    public class ModelFactory
    {

        public static OxyTimeGroupOnTheFlyStatsModel<TKey> CreateOnTheFlyStatsModel<TKey>(OxyPlotModel plotModel, bool createIfNotExists = true)
        {
            if (plotModel == null)
            {
                if (!createIfNotExists)
                    throw new NullReferenceException($"PlotModel is null and {createIfNotExists} equals false");
                else
                    plotModel = new OxyPlotModel();
            }

            return new OxyTimeGroupOnTheFlyStatsModel<TKey>(plotModel);
        }



        //public class OxyTimeModel<TKey> : Base.TimeModel<TKey>


        //public class OxyCartesianModel<TKey> : Base.CartesianModel<TKey>


        ////public class OxyTimeGroupModel<TGroupKey, TKey> : TimeGroupModel<TGroupKey, TKey>
        ////{
        ////    public OxyTimeGroupModel(OxyPlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) :
        ////        base(new TimePlotModel<TKey, ITimeGroupPoint<TGroupKey, TKey>>(plotModel), comparer, scheduler: scheduler)
        ////    {

        ////    }

        ////}

        //public class OxyTimeGroupModel<TKey>


        //public class OxyTime2KellyModel<TKey> : Time2KellyModel<TKey>


        //public class OxyTimeRangeModel<TKey> : TimeRangeModel<TKey>


        //public class OxyErrorBarModel : Base.ErrorBarModel


        //public class OxyCartesianGroupModel<TKey> : Cartesian.CartesianGroupModel<TKey>


        //public class OxyTimeOnTheFlyStatsModel<TKey> : Time.TimeOnTheFlyStatsModel<TKey>


        //public class OxyTimeLogGroupValueModel<TKey> : Time.TimeLogGroupValueModel<TKey>


        //public class OxyTimeLogGroupKeyModel : Time.TimeLogGroupKeyModel

        //public class OxyMultiTimePlotGroupStatsModel<TGroupKey, TKey> : MultiTimePlotGroupStatsModel<TGroupKey, TKey>


        //public class OxyMultiTimePlotKeyValueGroupStatsModel : Multi.MultiTimePlotKeyValueGroupStatsModel

        //public class OxyMultiTimePlotAccumulatedModel<TKey> : Multi.MultiTimePlotAccumulatedModel<TKey>


        //public class OxyMultiTimePlotAccumulatedModel : OxyMultiTimePlotAccumulatedModel<string>


    }
}

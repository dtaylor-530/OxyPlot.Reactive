using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxyPlot.Reactive
{
    public class OnTheFlyStatsHelper
    {
        public static ITimeStatsPoint<TKey> Combine<TKey>(ITime2Point<TKey, OnTheFlyStats.Stats> xy0, ITime2Point<TKey, OnTheFlyStats.Stats> xy, RollingOperation rollingOperation)
        {
            var value2 = xy0?.Value2 ?? new OnTheFlyStats.Stats();
            value2.Update(xy.Value);

            return rollingOperation switch
            {
                RollingOperation.Average => new TimeStatsPoint<TKey>(xy.Var, value2.Average, value2, xy.Key),
                RollingOperation.GeometricAverage => new TimeStatsPoint<TKey>(xy.Var, value2.GeometricAverage, value2, xy.Key),
                RollingOperation.SquareMean => new TimeStatsPoint<TKey>(xy.Var, value2.SquareMean, value2, xy.Key),
                RollingOperation.PopulationStandardDeviation => new TimeStatsPoint<TKey>(xy.Var, value2.PopulationStandardDeviation, value2, xy.Key),
                RollingOperation.SampleStandardDeviation => new TimeStatsPoint<TKey>(xy.Var, value2.SampleStandardDeviation, value2, xy.Key),
                RollingOperation.PopulationVariance => new TimeStatsPoint<TKey>(xy.Var, value2.PopulationVariance, value2, xy.Key),
                RollingOperation.SampleVariance => new TimeStatsPoint<TKey>(xy.Var, value2.SampleVariance, value2, xy.Key),
                RollingOperation.Max => new TimeStatsPoint<TKey>(xy.Var, value2.Max, value2, xy.Key),
                RollingOperation.Min => new TimeStatsPoint<TKey>(xy.Var, value2.Min, value2, xy.Key),
                RollingOperation.MidRange => new TimeStatsPoint<TKey>(xy.Var, value2.MidRange, value2, xy.Key),
                RollingOperation.Range => new TimeStatsPoint<TKey>(xy.Var, value2.Range, value2, xy.Key),
                RollingOperation.N => new TimeStatsPoint<TKey>(xy.Var, value2.N, value2, xy.Key),
                RollingOperation.Sum => new TimeStatsPoint<TKey>(xy.Var, value2.Sum, value2, xy.Key),
                RollingOperation.StandardError => new TimeStatsPoint<TKey>(xy.Var, value2.StandardError, value2, xy.Key),
                _ => new TimeStatsPoint<TKey>(xy.Var, value2.Average, value2, xy.Key),
            };
        }

    }
}

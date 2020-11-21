using ReactivePlot.Model;
using ReactivePlot.Model.Enum;

namespace ReactivePlot.Common
{
    public class OnTheFlyStatsHelper
    {
        public static ITimeStatsPoint<TKey> Combine<TKey>(ITimeModelPoint<TKey, OnTheFlyStats.Stats> xy0, ITimeModelPoint<TKey, OnTheFlyStats.Stats> xy, RollingOperation? rollingOperation = RollingOperation.Average)
        {
            var value2 = xy0?.Model ?? new OnTheFlyStats.Stats();
            value2.Update(xy.Value);

            return (rollingOperation ?? RollingOperation.Average) switch
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

        public static ITimeStatsGroupPoint<TGroupKey,TKey> Combine<TGroupKey,TKey>(ITimeStatsGroupPoint<TGroupKey, TKey> xy0, ITimeStatsGroupPoint<TGroupKey, TKey> xy, RollingOperation? rollingOperation = RollingOperation.Average)
        {
            var value2 = xy0?.Model ?? new OnTheFlyStats.Stats();
            value2.Update(xy.Value);

            return (rollingOperation ?? RollingOperation.Average) switch
            {
                RollingOperation.Average => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.Average, value2, xy.Key, xy.GroupKey),
                RollingOperation.GeometricAverage => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.GeometricAverage, value2, xy.Key, xy.GroupKey),
                RollingOperation.SquareMean => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.SquareMean, value2, xy.Key, xy.GroupKey),
                RollingOperation.PopulationStandardDeviation => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.PopulationStandardDeviation, value2, xy.Key, xy.GroupKey),
                RollingOperation.SampleStandardDeviation => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.SampleStandardDeviation, value2, xy.Key, xy.GroupKey),
                RollingOperation.PopulationVariance => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.PopulationVariance, value2, xy.Key, xy.GroupKey),
                RollingOperation.SampleVariance => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.SampleVariance, value2, xy.Key, xy.GroupKey),
                RollingOperation.Max => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.Max, value2, xy.Key, xy.GroupKey),
                RollingOperation.Min => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.Min, value2, xy.Key, xy.GroupKey),
                RollingOperation.MidRange => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.MidRange, value2, xy.Key, xy.GroupKey),
                RollingOperation.Range => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.Range, value2, xy.Key, xy.GroupKey),
                RollingOperation.N => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.N, value2, xy.Key, xy.GroupKey),
                RollingOperation.Sum => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.Sum, value2, xy.Key, xy.GroupKey),
                RollingOperation.StandardError => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.StandardError, value2, xy.Key, xy.GroupKey),
                _ => new TimeStatsGroupPoint<TGroupKey, TKey>(xy.Var, value2.Average, value2, xy.Key, xy.GroupKey),
            };
        }

    }
}

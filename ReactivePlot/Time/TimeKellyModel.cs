using CSharp.Pipe;
using OnTheFlyStats;
using ReactivePlot.Base;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;

namespace ReactivePlot.Time
{
    public class TimeKellyModel : TimeKellyModel<string>
    {
        public TimeKellyModel(IPlotModel<IKellyPoint<string>> model) : base(model)
        {
        }
    }


    public class TimeKellyModel<TKey> : TimeModel<TKey, IKellyPoint<TKey>>
    {
        public TimeKellyModel(IPlotModel<IKellyPoint<TKey>> model, IEqualityComparer<TKey> comparer = null, IScheduler scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IKellyPoint<TKey> CreatePoint(IKellyPoint<TKey> xy0, IKellyPoint<TKey> xy)
        {

            var kellyModel = xy0 == null ? new KellyModel
            {
                ProfitStats = new Stats(),
                KellyDictionary = new Dictionary<string, double>(),
                KellyProfits = new Stats(new double[] { 100d })
            } : xy0.Model;


            var current = xy.Odds > 0 ? xy.UnitProfit * kellyModel.KellyProfits.Sum *
                (kellyModel.KellyDictionary.GetValueOrDefault(xy.KellyKey ?? string.Empty).Pipe(a => a == default ? 0.01 : a)) / xy.Odds : 0;
            (kellyModel.KellyProfits ??= new Stats()).Update(current);

            kellyModel.ProfitStats.Update(xy.UnitProfit);

            var point = new KellyPoint<TKey>(xy.Var, kellyModel.KellyProfits.Sum, kellyModel, xy.Odds, xy.UnitProfit, xy.KellyKey, xy.Key);

            return point;
        }
    }


    public class Time2KellyModel<TKey> : TimeModel<TKey, IKellyPoint<TKey>>, IObserver<double>
    {
        private double ratio = 0.002;

        public Time2KellyModel(IPlotModel<IKellyPoint<TKey>> model, IEqualityComparer<TKey> comparer = null, IScheduler scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }


        protected override IKellyPoint<TKey> CreatePoint(IKellyPoint<TKey> xy0, IKellyPoint<TKey> xy)
        {
            return CreatePoint(xy0, xy, ratio);
        }

        public static IKellyPoint<TKey> CreatePoint(IKellyPoint<TKey> xy0, IKellyPoint<TKey> xy, double ratio)
        {

            var kellyModel = xy0 == null ? new KellyModel
            {
                ProfitStats = new Stats(),
                KellyDictionary = null,
                KellyProfits = new Stats(new double[] { 1d })
            } : xy0.Model;

            var sum = kellyModel.KellyProfits.Sum;
            var current = sum > 0 && xy.Odds > 0 ? xy.UnitProfit * sum * ratio : 0;
            (kellyModel.KellyProfits ??= new Stats()).Update(current);

            kellyModel.ProfitStats.Update(xy.UnitProfit);

            var point = new KellyPoint<TKey>(xy.Var, kellyModel.KellyProfits.Sum, kellyModel, xy.Odds, xy.UnitProfit, xy.KellyKey, xy.Key);

            return point;
        }

        public void OnNext(double value)
        {
            ratio = value;
            refreshSubject.OnNext(Unit.Default);
        }

    }


    public class KellyPoint<TKey> : IKellyPoint<TKey>
    {
        public KellyPoint(DateTime dateTime, double value, KellyModel model, double odds, double unitProfit, string kellyKey, TKey key)
        {
            Var = dateTime;
            Value = value;
            Model = model;
            Key = key;
            Odds = odds;
            UnitProfit = unitProfit;
        }

        public KellyPoint(DateTime dateTime, double value, KellyModel model, double odds, double unitProfit, string kellyKey) : this(dateTime, value, model, odds, unitProfit, kellyKey, default)
        {
        }

        public TKey Key { get; }

        public DateTime Var { get; }

        public double UnitProfit
        {
            get;
            set;
        }

        public double Odds { get; set; }

        public double Value { get; }

        public KellyModel Model { get; }

        public string KellyKey { get; set; }


        public override string ToString() => $"{Var:F}, {Value}, {Key}";

        //public static IKellyPoint<TKey> Create(DateTime dateTime, double value, KellyModel model, TKey key)
        //{
        //    return new KellyPoint<TKey>(dateTime, value, model, key);
        //}

    }

    public interface IKellyPoint<TKey> : ITimeModelPoint<TKey, KellyModel>
    {
        public double UnitProfit { get; set; }
        public double Odds { get; set; }

        public string KellyKey { get; set; }
    }


    public class KellyModel
    {
        //public double AverageProfit { get; set; }

        public Stats ProfitStats { get; set; }

        public Stats KellyProfits { get; set; }

        public Dictionary<string, double> KellyDictionary { get; set; }
    }
}

using CSharp.Pipe;
using MoreLinq;
using OnTheFlyStats;
using ReactivePlot.Base;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace ReactivePlot.Time
{
    //public class TimeKellyModel : TimeKellyModel<string>
    //{
    //    public TimeKellyModel(IMultiPlotModel<IKellyPoint<string>> model) : base(model)
    //    {
    //    }
    //}

    public class TimeKellyModel<TKey> : TimeModel<TKey, IProfitPoint<TKey>, IKellyModelPoint<TKey>>, IObserver<KellyModel>
    {
        private KellyState state;
        private KellyModel? kellyModel;

        public TimeKellyModel(KellyModel kellyModel, IMultiPlotModel<IKellyModelPoint<TKey>> model, IEqualityComparer<TKey> comparer = null, IScheduler scheduler = null) : this(model, comparer, scheduler: scheduler)
        {
            state = new KellyState(kellyModel.Fraction);
            this.kellyModel = kellyModel;
        }

        public TimeKellyModel(IMultiPlotModel<IKellyModelPoint<TKey>> model, IEqualityComparer<TKey> comparer = default, IScheduler scheduler = default) : base(model, comparer, scheduler: scheduler)
        {
            const double fraction = 0.005;
            state = new KellyState(fraction);
            this.kellyModel = new KellyModel(new KellyConfiguration(1, fraction));
        }

        protected override IKellyModelPoint<TKey> CreateNewPoint(IKellyModelPoint<TKey> xy0, IProfitPoint<TKey> xy)
        {
            if (xy.Odd > 0)
            {
                var current = state.GetProfit(xy.Odd, xy.UnitProfit, kellyModel.KellyProfits.Sum);
                kellyModel.KellyProfits.Update(current);
                kellyModel.ProfitStats.Update(xy.UnitProfit);

                if (kellyModel.KellyProfits.Sum < 0)
                {

                }
                var point = new KellyModelPoint<TKey>(xy.Var, kellyModel, xy.Odd, xy.UnitProfit, default, xy.Key);
                return point;
            }

            return new KellyModelPoint<TKey>(xy.Var, kellyModel, xy.Value, 0, default, xy.Key); ;
        }

        protected override IEnumerable<IKellyModelPoint<TKey>> ToDataPoints(IEnumerable<IProfitPoint<TKey>> collection)
        {
            var first = collection.First();
            var arr =
           collection
           .Scan((IKellyModelPoint<TKey>)new KellyModelPoint<TKey>(first.Var, kellyModel, first.Odd, first.UnitProfit, default, first.Key),
           (a, b) => CreateNewPoint(a, b)).ToArray();

           var total= collection
           .Aggregate(1d, (sum, xy) =>
            {
                var current = state.GetProfit(xy.Odd, xy.UnitProfit, sum);
                sum += current;
                return sum;
            });

            return arr;
        }

        public void OnNext(KellyModel value)
        {
            kellyModel = value;
            state = new KellyState(value.Fraction);
            refreshSubject.OnNext(Unit.Default);
        }

        protected override IProfitPoint<TKey> CreatePoint(IProfitPoint<TKey> xy0, IProfitPoint<TKey> xy)
        {
            throw new NotImplementedException();
        }

        protected override TKey GetKey(IProfitPoint<TKey> item)
        {
            throw new NotImplementedException();
        }
    }

    public class KellyModel<TKey> : TimeModel<TKey, IProfitPoint<TKey>, Point<double>>
    {
        public KellyModel(IMultiPlotModel<Point<double>> model, IEqualityComparer<TKey> comparer = null, IScheduler scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<Point<double>> ToDataPoints(IEnumerable<IProfitPoint<TKey>> collection)
        {
            return GetPointProfit(collection).AsParallel();
        }

        protected static IEnumerable<Point<double>> GetPointProfit(IEnumerable<IProfitPoint<TKey>> collection)
        {
            //   IProfitPoint<TKey>[] profitPoints = Enumerable.Repeat(collection.OrderBy(a => new Guid()).ToArray(), 10).SelectMany(a => a).ToArray();
            var arr = collection.ToArray();
            for (int i = 0; i < 10000; i++)
            {
                var state = new KellyState(i / 10000d);
                double sum = 1;
                var count = arr.Length;// random.Next(0, arr.Length - 1);
                //foreach (var xy in arr.OrderBy(a=>new Guid()).Take(count))
                foreach (var xy in arr)
                {
                    if (xy.Odd > 1)
                    {
                        var current = state.GetProfit(xy.Odd, xy.UnitProfit, sum);
                        sum += current;
                    }
                    else if (xy.Odd == 1)
                    {

                    }
                    else
                    {
                        //throw new Exception("sdfdf");
                    }
                }

                yield return new Point<double>(i / 10000d, sum / count);
            }
        }

        protected override Point<double> CreateNewPoint(Point<double> xy0, IProfitPoint<TKey> xy)
        {
            throw new NotImplementedException();
        }

        protected override IProfitPoint<TKey> CreatePoint(IProfitPoint<TKey> xy0, IProfitPoint<TKey> xy)
        {
            throw new NotImplementedException();
        }

        protected override TKey GetKey(IProfitPoint<TKey> item)
        {
            throw new NotImplementedException();
        }
    }


    class KellyState
    {
        private readonly double fraction;

        public KellyState(double fraction)
        {
            this.fraction = fraction;
        }

        public double GetProfit(double odd, double unitProfit, double sum)
        {
            var current = unitProfit * sum * fraction / odd;
            return current;
        }
    }

    public class KellyState2
    {
        private readonly double winPercentage;
        private readonly Random random;

        public KellyState2(double winPercentage, Random? random = default)
        {
            this.winPercentage = winPercentage;
            this.random = random ?? new Random();
        }

        public double GetUnitProfit(double odd)
        {
            var tRandom = random.NextDouble();
            double va = (1 / odd + winPercentage);
            var winLoss = tRandom != va ? tRandom < va ? (bool?)true : false : null;
            double unitProfit = winLoss.HasValue ? winLoss.Value ? odd - 1 : -1 : 0;
            return unitProfit;
        }
    }


    public struct KellyConfiguration
    {
        //, , double oddMean, double oddDeviation,
        public KellyConfiguration(double initialBalance, double fraction)//, double profitablity, double winPercentage)
        {
            InitialBalance = initialBalance;
            Fraction = fraction;
        }

        public double InitialBalance { get; }
        public double Fraction { get; }

    }

    public class KellyModelPoint<TKey> : ProfitPoint<TKey>, IKellyModelPoint<TKey>
    {
        public KellyModelPoint(DateTime dateTime, KellyModel? model, double odd, double unitProfit, string? kellyKey, TKey key) :
             base(dateTime, odd, unitProfit, model?.KellyProfits.Sum ?? 0, kellyKey, default)
        {
            Model = model;
        }

        //public KellyModelPoint(DateTime dateTime, KellyModel model, double odd, double unitProfit, string? kellyKey) :
        //    this(dateTime, model, odd, unitProfit, kellyKey, default)
        //{
        //}


        public KellyModel Model { get; }


    }

    public class ProfitPoint<TKey> : IProfitPoint<TKey>
    {

        public ProfitPoint(DateTime dateTime, double odd, double unitProfit, double value, string? kellyKey, TKey key) :
            this(dateTime, odd, unitProfit, kellyKey, key)
        {
            Value = value;
        }

        public ProfitPoint(DateTime dateTime, double odd, double unitProfit, string? kellyKey, TKey key)
        {
            Var = dateTime;
            Odd = odd;
            Key = key;
            UnitProfit = unitProfit;
            KellyKey = kellyKey;
        }

        //public KellyPoint(DateTime dateTime, double odd, double unitProfit, string? kellyKey) :
        //    this(dateTime, odd, unitProfit, kellyKey, default)
        //{
        //}

        public TKey Key { get; }

        public DateTime Var { get; }

        public double UnitProfit { get; }

        public double Odd { get; }

        public double Value { get; }

        public string? KellyKey { get; }


        public override string ToString() => $"{Var:F}, {Value}, {Key}";

        //public static IKellyPoint<TKey> Create(DateTime dateTime, double value, KellyModel model, TKey key)
        //{
        //    return new KellyPoint<TKey>(dateTime, value, model, key);
        //}
    }

    public interface IProfitPoint<TKey> : ITimePoint<TKey>
    {
        public double UnitProfit { get; }

        public double Odd { get; }

    }

    public interface IKellyModelPoint<TKey> : ITimeModelPoint<TKey, KellyModel>
    {
        public double UnitProfit { get; }

        public double Odd { get; }

        public string KellyKey { get; }
    }

    public class KellyModel
    {
        public KellyModel(double fraction, Stats profitStats, Stats kellyProfits, Dictionary<string, double> kellyDictionary)
        {
            Fraction = fraction;
            ProfitStats = profitStats;
            KellyProfits = kellyProfits;
            KellyDictionary = kellyDictionary;
        }

        public KellyModel(double fraction, double initialBalance)
        {
            Fraction = fraction;
            ProfitStats = new Stats();
            KellyProfits = new Stats(new[] { initialBalance });
            KellyDictionary = new Dictionary<string, double>();
        }
        public KellyModel(KellyConfiguration kellyConfiguration)
        {
            Fraction = kellyConfiguration.Fraction;
            ProfitStats = new Stats();
            KellyProfits = new Stats(new[] { kellyConfiguration.InitialBalance });
            KellyDictionary = new Dictionary<string, double>();
        }

        public double Fraction { get; }

        public Stats ProfitStats { get; }

        public Stats KellyProfits { get; }

        public Dictionary<string, double> KellyDictionary { get; }
    }
}

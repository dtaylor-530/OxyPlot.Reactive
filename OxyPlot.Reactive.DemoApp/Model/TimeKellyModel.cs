using Endless;
using OnTheFlyStats;
using OxyPlot.Axes;
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace OxyPlot.Reactive.DemoApp.Model
{

    public class TimeKellyModel : TimeKellyModel<string>
    {
        public TimeKellyModel(PlotModel model) : base(model)
        {
        }
    }


    public class TimeKellyModel<TKey> : Time2Model<TKey, ITimeKellyPoint<TKey>>
    {

        public TimeKellyModel(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override ITimeKellyPoint<TKey> CreatePoint(ITimeKellyPoint<TKey> xy0, ITimeKellyPoint<TKey> xy)
        {
            var diff = xy.Profit != 0 ? (xy.Profit > 0 ? 1 : 1 - xy.Odd) : 0;

            var cumuDiff = xy0?.CumuProfitStats?.Average ?? 0;

            var positiveDiff = 0.5 + cumuDiff;
            var negativeDiff = 0.5 - cumuDiff;
            var odd = xy.Odd;
            var modificationFactor = (1 - Math.Exp(-Math.Log(xy0?.CumuProfitStats?.N ?? 1, 1000000000)));
            var kelly = odd == 0 ? 0 : modificationFactor * (positiveDiff * (1) - negativeDiff * (odd - 1)) / odd;

            Stats x, y, z, x_;
            (x = xy0?.OddStats ?? new Stats()).Update(xy.Odd);
            //(y = xy0?.ProfitStats ?? new Stats()).Update(();
            (z = xy0?.WagerStats ?? new Stats()).Update(xy.Wager);
            (x_ = xy0?.CumuProfitStats ?? new Stats()).Update(diff);

            var sum = xy0?.Value ?? 100;

            var cumuProfit = kelly > 0 && sum > 0 ? (sum * kelly * diff + sum) : sum;
            if (cumuProfit < 0)
            {

            }
            double xa, ya, za;
            xa = xy?.Odd ?? 0;
            ya = xy?.Profit ?? 0;
            za = xy?.Wager ?? 0;

            var point = new TimeKellyPoint<TKey>(xy.Var, x_, null, z, x, ya, za, xa, cumuProfit, xy.Key);

            return point;
        }
    }

    public abstract class Time2Model<TKey, R> : TimeModel<TKey, R, R> where R : ITimeKellyPoint<TKey>
    {
        public Time2Model(PlotModel model, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, comparer, scheduler: scheduler)
        {
        }

        protected override IEnumerable<R> ToDataPoints(IEnumerable<KeyValuePair<TKey, R>> collection)
        {
            return
                collection
                .Select(a => a.Value)
                .Select(a => { return a; })
                .Scan(seed: default(R), (a, b) => CreatePoint(a, b))
                .Skip(1);
        }


    }


    public class TimeKellyPoint<TKey> : ITimeKellyPoint<TKey>
    {
        public TimeKellyPoint(DateTime dateTime, Stats cumuProfitStats, Stats profitStats, Stats wageredStats, Stats oddsStats, double profit, double wager, double odd, double cumuProfit, TKey key)
        {
            Var = dateTime;
            CumuProfitStats = cumuProfitStats;
            ProfitStats = profitStats;
            WagerStats = wageredStats;
            OddStats = oddsStats;
            Profit = profit;
            Wager = wager;
            Odd = odd;
            this.Key = key;


            var diff = cumuProfitStats?.Average ?? 0;
            //var diff = (ProfitStats?.Sum / (WagerStats?.Sum)) / 2;

            Value = cumuProfit;

            //var modificationFactor = (1 - Math.Exp(-Math.Log(cumuProfitStats?.N ?? 1, 1000))) / 2;
            //var modifiedKelly = kelly * modificationFactor;
            //Value = modifiedKelly;// * Math.Max(CumuProfitStats?.Sum ?? 0,0) * Profit;

        }


        public TKey Key { get; }

        public DateTime Var { get; }

        public double Value { get; }

        public Stats ProfitStats { get; }

        public Stats CumuProfitStats { get; }

        public Stats WagerStats { get; }

        public Stats OddStats { get; }

        public double Wager { get; }

        public double Odd { get; }

        public double Profit { get; }


        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(Var), Value);
        }

        public override string ToString() => $"{Var:F}, {Value}, {Key}";

    }

    public interface ITimeKellyPoint<TKey> : ITimePoint<TKey>
    {
        Stats OddStats { get; }
        Stats ProfitStats { get; }
        Stats WagerStats { get; }

        double Odd { get; }
        double Profit { get; }
        double Wager { get; }
        Stats CumuProfitStats { get; }
    }

}

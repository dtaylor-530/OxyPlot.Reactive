using OxyPlot.Reactive.DemoApp.Common;
using ReactivePlot.Time;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactivePlot.Data.Common;
using HandyControl.Data;
using ReactivePlot.Model;
using ReactivePlot.OxyPlot.PlotModel;
using ReactivePlot.Ex;

namespace OxyPlot.Reactive.DemoApp.Views
{
    using static MathNetExtensions;

    /// <summary>
    /// Interaction logic for TimeSeriesStatsView.xaml
    /// </summary>
    public partial class TimeKellyView : UserControl
    {
        CompositeDisposable disposable = new CompositeDisposable();

        public TimeKellyView()
        {
            InitializeComponent();
   
            ToggleContent
                 .SelectToggleChanges()
                 .Subscribe(a =>
                 {
                     disposable.Dispose();
                     disposable = new CompositeDisposable();
                     var kellys = GetConfigs();
                     var models = kellys.Select(config => new KellyModel(config)); 
                     SetConfigs2(kellys);
                     var samples = GetData(a);
                     SetTimeKellyModel(samples, models);
                     SetTimeKellyModel2(samples, models);
                     SetKellyModel(samples);
                 });
        }

        IObservable<ProfitPoint<string>[]> GetData(bool a)
        {
            IObservable<ProfitPoint<string>[]> samples;
            if (!a)
            {
                samples = GetSimulatedData(out var configs);
                SetConfigs(configs);
            }
            else
            {
                samples = GetCsvData(new Csv().Read().Take(1000).ToArray());
            }
            return samples;
        }


        void SetConfigs(IObservable<BetConfiguration> configs)
        {
            configs
                .BindTo(PropertyGridConfig, a => a.SelectedObject)
                .DisposeWith(disposable);
        }      
        
        void SetConfigs2(IObservable<KellyConfiguration> kellys)
        {
            kellys
                .BindTo(PropertyGridConfig2, a => a.SelectedObject)
                .DisposeWith(disposable);
        }

        void SetTimeKellyModel(IObservable<ProfitPoint<string>[]> samples, IObservable<KellyModel> configs)
        {
            var dateTime = DateTime.Now;

            var md = new OxyTimePlotModel<string, IKellyModelPoint<string>>(plotView1.Model ??= new PlotModel());

            var model = new TimeKellyModel<string>(md, scheduler: RxApp.MainThreadScheduler);

            samples
                .SelectMany(a => a)
                .Subscribe(a =>
                {
                    model.OnNext(new HashSet<string>());
                    model.OnNext(KeyValuePair.Create("", (IProfitPoint<string>)a));
                })
                .DisposeWith(disposable);

            _ = configs.Subscribe(model)
                        .DisposeWith(disposable);
        }       
        
        void SetTimeKellyModel2(IObservable<ProfitPoint<string>[]> samples, IObservable<KellyModel> configs)
        {
            var dateTime = DateTime.Now;

            var md = new DataGridPlotModel<DateTime,IKellyModelPoint<string>>(DataGrid1);

            var model = new TimeKellyModel<string>(md, scheduler: RxApp.MainThreadScheduler);

            samples
                .SelectMany(a => a)
                .Subscribe(a =>
                {
                    model.OnNext(new HashSet<string>());
                    model.OnNext(KeyValuePair.Create("", (IProfitPoint<string>)a));
                })
                .DisposeWith(disposable);

            _ = configs.Subscribe(model)
                        .DisposeWith(disposable);
        }

        void SetKellyModel(IObservable<ProfitPoint<string>[]> samples)
        {
            var md2 = new OxyCartesianPlotModel<string, Point<double>>(plotView2.Model ??= new PlotModel());

            var model = new KellyModel<string>(md2, scheduler: RxApp.MainThreadScheduler);

            samples
                .SelectMany(a => a)
             .Subscribe(a =>
             {
                 model.OnNext(new HashSet<string>());
                 model.OnNext(KeyValuePair.Create("", (IProfitPoint<string>)a));
             })
             .DisposeWith(disposable);
        }


        IObservable<KellyConfiguration> GetConfigs()
        {
            var configs = BalanceNumericUpDown.SelectValueChanges()
              .CombineLatest(FractionNumericUpDown.SelectValueChanges(),
              (a, b) => (a, b))
              .Select(d =>
              {
                  var config = new KellyConfiguration(d.a, d.b / 100d);
                  return config;
              });

            return configs;
        }

        IObservable<ProfitPoint<string>[]> GetSimulatedData(out IObservable<BetConfiguration> configs)
        {
            var dateTime = DateTime.Now;
            Random random = new Random();

            configs = OddMeanNumericUpDown
                .SelectValueChanges()
                .CombineLatest(
                OddDeviationNumericUpDown.SelectValueChanges(),
                CountNumericUpDown.SelectValueChanges(),
                (a, b, c) => new BetConfiguration(a, b, c));

            var profits = configs.CombineLatest(WinPercentageNumericUpDown.SelectValueChanges(), (a, b) => (a, b)).Select(ae =>
            {
                (BetConfiguration a, double b) = ae;
                Point<int>[] sa = NormalSamples(1 / a.Mean, a.Deviation, (int)a.Count).Where(a => a > 1).Select((a, i) => new Point<int>(i, Math.Truncate(a * 100d) / 100d)).ToArray();

                var kelly2 = new KellyState2(b / 1000d, random);

                return sa.Select((b, i) =>
                {
                    var profit = kelly2.GetUnitProfit(b.Value);
                    return new ProfitPoint<string>(dateTime.AddDays(i), b.Value, profit, default, default);
                }).ToArray();
            });
            return profits;
        }

        static IObservable<ProfitPoint<string>[]> GetCsvData(CsvRow[] csvRows)
        {
            var csv = csvRows.Select(a => new ProfitPoint<string>(a.DateTime_, a.Odd, LayUnitProfit(a), "", ""))
          .OrderBy(a => a.Var);

            var merge = Observable.Return(csv.Take(500).ToArray())
                 .Merge(
                Observable.Return(csv
                 .Skip(500)
                 .ToArray()));

            IObservable<ProfitPoint<string>[]> cc =
                merge
                .Pace(TimeSpan.FromSeconds(0.5))
                 .Publish().RefCount();
            return cc;

            static double LayUnitProfit(CsvRow csvRow)
            {
                return csvRow.Profit > 0 ? 1 : 1 - csvRow.Odd;
            }
        }

    }

    static class HandyExtension
    {
        public static IObservable<double> SelectValueChanges(this HandyControl.Controls.NumericUpDown numericUpDown)
        {
            return Observable.FromEventPattern<
                EventHandler<FunctionEventArgs<double>>,
                FunctionEventArgs<double>>(a => numericUpDown.ValueChanged += a, a => numericUpDown.ValueChanged -= a)
                .Select(a => a.EventArgs.Info)
                .StartWith(numericUpDown.Value);
        }
    }

    static class MathNetExtensions
    {
        static readonly Random random = new Random();

        public static double[] NormalSamples(double mean, double deviation, int count)
        {

            var array = MathNet.Numerics.Distributions.Normal.Samples(random, mean, deviation).Take(count).ToArray();
            return array;
        }
    }

    public struct BetConfiguration
    {
        public BetConfiguration(double mean, double deviation, double count)
        {
            Mean = mean;
            Deviation = deviation;
            Count = count;
        }

        public double Mean { get; }
        public double Deviation { get; }
        public double Count { get; }
    }
}

using OxyPlot.Data.Common;
using OxyPlot.Data.Factory;
using ReactiveUI;
using System;
using System.Windows;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Model.Enum;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Controls;
using OxyPlot.Reactive.DemoApp.Common;
using System.Reactive.Linq;
using OxyPlot.Reactive.DemoApp.Model;
using System.Resources;
using System.Collections.Generic;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for TimeSeriesStatsView.xaml
    /// </summary>
    public partial class TimeSeriesStatsView : UserControl
    {
        CompositeDisposable disposable = new CompositeDisposable();

        public TimeSeriesStatsView()
        {
            InitializeComponent();

            var model1 = new TimeOnTheFlyStatsModel<string>(plotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model2 = new TimeOnTheFlyStatsModel<string>(plotView2.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model3 = new TimeOnTheFlyStatsModel<string>(plotView3.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model4 = new TimeOnTheFlyStatsModel<string>(plotView4.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model5 = new TimeKellyModel<string>(plotView5.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            disposable = new CompositeDisposable();

            TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.5))
               .SubscribeCustom3<ITimeTwoPoint<string, OnTheFlyStats.Stats>, ITimeTwoPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model1)
               .DisposeWith(disposable);

            TimeDataSource.Observe3()
                .SubscribeCustom3<ITimeTwoPoint<string, OnTheFlyStats.Stats>, ITimeTwoPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model2)
                     .DisposeWith(disposable);

            TimeDataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(0.2))
               .SubscribeCustom3<ITimeTwoPoint<string, OnTheFlyStats.Stats>, ITimeTwoPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model3)
                    .DisposeWith(disposable);

            TimeDataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(0.2))
               .SubscribeCustom3<ITimeTwoPoint<string, OnTheFlyStats.Stats>, ITimeTwoPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model4)
                    .DisposeWith(disposable);

            TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.5))
        .SubscribeCustom3<ITimeTwoPoint<string, OnTheFlyStats.Stats>, ITimeTwoPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model1)
        .DisposeWith(disposable);


            var array = Csv.Read()
                 .Select(a => new TimeKellyPoint<string>(a.DateTime_, null, null, null, null, a.Profit, a.Wager, a.Odd, 0, ""))
                 .ToArray();

            array.Take(1000).ToObservable().Merge(
                array
                .Skip(1000)
                .ToObservable()
                .Pace(TimeSpan.FromSeconds(0.1)))
                .Subscribe(a => model5.OnNext(KeyValuePair.Create("", (ITimeKellyPoint<string>)a)))
                .DisposeWith(disposable);

            ComboBox1.SelectionChanged += (s, e) =>
            {
                var rollingOperation = e.AddedItems.Cast<RollingOperation>().Single();
                model1.OnNext(rollingOperation);
                model2.OnNext(rollingOperation);
                model3.OnNext(rollingOperation);
                model4.OnNext(rollingOperation);
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }
    }

}

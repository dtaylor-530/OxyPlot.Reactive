using OxyPlot.Reactive;
using OxyPlot.Data.Common;
using OxyPlot.Data.Factory;
using ReactiveUI;
using System;
using System.Windows;
using OxyPlot.Reactive.Model;
using OxyPlot.Reactive.Model.Enum;
using System.Linq;
using OxyPlot.Reactive.Time;
using System.Reactive.Disposables;
using System.Windows.Controls;

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

            var model1 = new TimeRollingStatisticsModel<string>(plotView.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model2 = new TimeRollingStatisticsModel<string>(plotView2.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model3 = new TimeRollingStatisticsModel<string>(plotView3.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model4 = new TimeRollingStatisticsModel<string>(plotView4.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);

            disposable = new CompositeDisposable();

             TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.5))
                .SubscribeCustom3<ITime2Point<string, OnTheFlyStats.Stats>, ITime2Point<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model1)
                .DisposeWith(disposable);

            TimeDataSource.Observe3()
                .SubscribeCustom3<ITime2Point<string, OnTheFlyStats.Stats>, ITime2Point<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model2)
                     .DisposeWith(disposable);

             TimeDataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(0.2))
                .SubscribeCustom3<ITime2Point<string, OnTheFlyStats.Stats>, ITime2Point<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model3)
                     .DisposeWith(disposable);

             TimeDataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(0.2))
                .SubscribeCustom3<ITime2Point<string, OnTheFlyStats.Stats>, ITime2Point<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model4)
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

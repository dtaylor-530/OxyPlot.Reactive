using OxyPlot.Reactive;
using OxyPlot.Data.Common;
using OxyPlot.Data.Factory;
using ReactiveUI;
using System;
using System.Windows;
using System.Reactive.Disposables;

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for MultiDateTimeModelAccumulatedView.xaml
    /// </summary>
    public partial class MultiDateTimeModelAccumulatedView
    {
        private readonly System.Reactive.Disposables.CompositeDisposable disposable = new System.Reactive.Disposables.CompositeDisposable();

        public MultiDateTimeModelAccumulatedView()
        {
            InitializeComponent();
            var model1 = new TimeAccumulatedGroupModel<string, string>(plotView.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model2 = new TimeAccumulatedModel<string>(plotView2.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model3 = new TimeAccumulatedModel<string>(plotView3.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            //var model3 = new TimeRollingStatisticsModel<string>(plotView3.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            //var model4 = new TimeRollingStatisticsModel<string>(plotView4.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);

           TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.5))
                .SubscribeCustom(model1)
                .DisposeWith(disposable);

           TimeDataSource.Observe3()
                .SubscribeCustom(model2)
                .DisposeWith(disposable);

            TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.2))
        .SubscribeCustom(model3)
        .DisposeWith(disposable);

            //disposable = TimeDataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(1.5))
            //    .SubscribeCustom3<ITime2Point<string, OnTheFlyStats.Stats>, ITime2Point<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model3);

            //disposable = TimeDataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(1.5))
            //    .SubscribeCustom3<ITime2Point<string, OnTheFlyStats.Stats>, ITime2Point<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model4);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }

    }
}
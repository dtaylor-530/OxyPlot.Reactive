﻿using OxyPlot.Reactive.DemoApp.Common;
using ReactivePlot.Data.Factory;
using ReactivePlot.Model;
using ReactivePlot.Model.Enum;
using ReactivePlot.Time;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactivePlot.Data.Common;
using ReactivePlot.Common;
using ReactivePlot.OxyPlot;

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

            var model1 = new OxyTimeOnTheFlyStatsModel<string>(plotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model2 = new OxyTimeOnTheFlyStatsModel<string>(plotView2.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model3 = new OxyTimeOnTheFlyStatsModel<string>(plotView3.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model4 = new OxyTimeOnTheFlyStatsModel<string>(plotView4.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            disposable = new CompositeDisposable();

            TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.5))
               .SubscribeCustom3<ITimeModelPoint<string, OnTheFlyStats.Stats>, ITimeModelPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model1)
               .DisposeWith(disposable);

            TimeDataSource.Observe3()
                .SubscribeCustom3<ITimeModelPoint<string, OnTheFlyStats.Stats>, ITimeModelPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model2)
                .DisposeWith(disposable);

            TimeDataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(0.2))
               .SubscribeCustom3<ITimeModelPoint<string, OnTheFlyStats.Stats>, ITimeModelPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model3)
               .DisposeWith(disposable);

            TimeDataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(0.2))
               .SubscribeCustom3<ITimeModelPoint<string, OnTheFlyStats.Stats>, ITimeModelPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model4)
               .DisposeWith(disposable);

            TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.5))
                .SubscribeCustom3<ITimeModelPoint<string, OnTheFlyStats.Stats>, ITimeModelPoint<string, OnTheFlyStats.Stats>, OnTheFlyStats.Stats>(model1)
                .DisposeWith(disposable);


            ComboBox1.SelectItemChanges<RollingOperation>().Subscribe(rollingOperation =>
            {
                model1.OnNext(rollingOperation);
                model2.OnNext(rollingOperation);
                model3.OnNext(rollingOperation);
                model4.OnNext(rollingOperation);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }
    }

}

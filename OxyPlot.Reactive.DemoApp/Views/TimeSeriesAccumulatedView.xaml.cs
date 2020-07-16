using OxyPlot.Reactive;
using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.Factory;
using ReactiveUI;
using System;
using System.Windows;

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for MultiDateTimeModelAccumulatedView.xaml
    /// </summary>
    public partial class MultiDateTimeModelAccumulatedView
    {
        private readonly IDisposable disposable;


        public MultiDateTimeModelAccumulatedView()
        {
            InitializeComponent();
            //ProduceData(out var observable1, out var observable2);


            disposable = DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.5)).Subscribe(
                new TimeAccumulatedModel<string>(plotView.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler)); ;



            disposable = DataSource.Observe3().Subscribe(
                new TimeAccumulatedModel<string>(plotView2.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler));

            disposable = DataSource.Observe1000PlusMinus().Pace(TimeSpan.FromSeconds(1.5)).Subscribe(
     new TimeAccumulatedModel<string>(plotView3.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler));

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }
    }
}

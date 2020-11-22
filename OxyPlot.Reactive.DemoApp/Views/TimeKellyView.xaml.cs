using OxyPlot.Reactive.DemoApp.Common;
using ReactivePlot.OxyPlot;
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

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for TimeSeriesStatsView.xaml
    /// </summary>
    public partial class TimeKellyView : UserControl
    {
        CompositeDisposable disposable = new CompositeDisposable();

        public TimeKellyView()
        {
            InitializeComponent();

            //var model = new TimeKellyModel<string>(plotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);
            var model2 = new OxyTime2KellyModel<string>(plotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            model2.OnNext(0.02);

            disposable = new CompositeDisposable();

            var array = new Csv().Read()
           .Take(1000).ToArray();

            DataGrid1.ItemsSource = array;

            GetData(array)
                .Subscribe(a => model2.OnNext(KeyValuePair.Create("", (IKellyPoint<string>)a)))
                .DisposeWith(disposable);

            RatioComboBox.SelectItemChanges<double>().Subscribe(model2.OnNext);
        }

        static IObservable<KellyPoint<string>> GetData(CsvRow[] csvRows)
        {

            var csv = csvRows.Select(a => new KellyPoint<string>(a.DateTime_, default, default, a.Odd, LayUnitProfit(a), ""))
          .OrderBy(a => a.Var);

            IObservable<KellyPoint<string>> cc = csv
                .Take(500)
                .ToObservable()
                .Merge(
                csv
                .Skip(500)
                .ToObservable()
                .Pace(TimeSpan.FromSeconds(0.5)))
                 .Publish().RefCount();
            return cc;
        }

        static double LayUnitProfit(CsvRow csvRow)
        {
            return csvRow.Profit > 0 ? 1 : 1 - csvRow.Odd;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }
    }

}

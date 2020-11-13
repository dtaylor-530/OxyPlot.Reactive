using OxyPlot.Data.Common;
using ReactiveUI;
using System;
using System.Windows;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Controls;
using OxyPlot.Reactive.DemoApp.Common;
using System.Reactive.Linq;
using System.Collections.Generic;
using OxyPlot.Reactive.Time;

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
            var model2 = new Time2KellyModel<string>(plotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            disposable = new CompositeDisposable();

            GetData()
                .Subscribe(a => model2.OnNext(KeyValuePair.Create("", (IKellyPoint<string>)a)))
                .DisposeWith(disposable);

            RatioComboBox.SelectItemChanges<double>().Subscribe(model2.OnNext);
        }

        static IObservable<KellyPoint<string>> GetData()
        {
            var array = Csv.Read()
                .Select(a => new KellyPoint<string>(a.DateTime_, default, null, a.Odd, LayUnitProfit(a), ""))
                .OrderBy(a=>a.Var)
                .ToArray();

            IObservable<KellyPoint<string>> cc = array.Take(2000).ToObservable().Merge(
                array
                .Skip(2000)
                .ToObservable()
                .Pace(TimeSpan.FromSeconds(0.1)))
                 .Publish().RefCount();
            return cc;
        }

        static double LayUnitProfit(CsvRow csvRow)
        {
            return csvRow.Profit > 0 ? 1 : -csvRow.Odd;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }
    }

}

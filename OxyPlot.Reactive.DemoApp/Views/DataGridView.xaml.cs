using OxyPlot.Reactive.DemoApp.Common;
using ReactivePlot.Ex;
using ReactivePlot.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ReactivePlot.Data.Common;
using ReactiveUI;

namespace ReactivePlot.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for DataGridView.xaml
    /// </summary>
    public partial class DataGridView : Page
    {
        public DataGridView()
        {
            InitializeComponent();

            var md = new DataGridPlotModel<DateTime, IKellyModelPoint<string>>(DataGrid1);
            var model = new TimeKellyModel<string>(md, scheduler: RxApp.MainThreadScheduler);

            GetData(true)
                .SelectMany(a => a)
                .Subscribe(a =>
                {
                    model.OnNext(new HashSet<string>());
                    model.OnNext(KeyValuePair.Create("sdsdsd sdsd", (IProfitPoint<string>)a));
                })
                .DisposeWith(new CompositeDisposable());

            //_ = configs.Subscribe(model)
            //            .DisposeWith(disposable);
        }


        IObservable<ProfitPoint<string>[]> GetData(bool a)
        {
            IObservable<ProfitPoint<string>[]> samples;
  
             samples = GetCsvData(new Csv().Read().Take(1000).ToArray());
            
            return samples;
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
}

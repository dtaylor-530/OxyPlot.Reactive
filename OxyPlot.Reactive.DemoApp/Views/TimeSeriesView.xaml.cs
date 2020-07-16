using DynamicData;
using OxyPlot.Reactive;
using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.Factory;
using OxyPlot.Reactive.DemoApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for LineView.xaml
    /// </summary>
    public partial class MultiDateTimeModelView : UserControl
    {
        public MultiDateTimeModelView()
        {
            InitializeComponent();
            var pacedObs = DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.1));

            pacedObs.Subscribe(new TimeModel<string>(plotView.Model ??= new OxyPlot.PlotModel(), scheduler: ReactiveUI.RxApp.MainThreadScheduler) { });

            var model2 = new TimeModel<string>(plotView2.Model??= new OxyPlot.PlotModel(), scheduler: ReactiveUI.RxApp.MainThreadScheduler) { };

            var obs = DataSource.Observe20();
            obs.Subscribe(model2);
            obs
                  .ToObservableChangeSet()
                .Bind(out var collection)
                .Subscribe();
               


            model2.Subscribe(p =>
            {
                var n = collection.Select((a, i) => (a.Value.Key, i)).Single(a => a.Item1 == p.DateTime).i;
                DataGrid1.SelectedIndex = n;
                DataGrid1.ScrollIntoView(DataGrid1.Items[n]);
            });

            DataGrid1.ItemsSource = collection;

            var obs2 = pacedObs.Select(a => (KeyValuePair<string, KeyValuePair<DateTime, double>>?)a).Delay(TimeSpan.FromSeconds(5)).StartWith(default(KeyValuePair<string, KeyValuePair<DateTime, double>>?));
            ViewModelViewHost1.ViewModel = new BusyViewModel(obs2);

            DataSource.Observe1000().Concat(DataSource.Observe1000()).Subscribe(new TimeModel<string>(plotView1.Model ??= new OxyPlot.PlotModel(), scheduler: ReactiveUI.RxApp.MainThreadScheduler) { });
        }
    }
}

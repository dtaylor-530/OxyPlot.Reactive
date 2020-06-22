using DynamicData;
using OxyPlot.Reactive;
using OxyPlot.Reactive.DemoApp.Factory;
using OxyPlot.Reactive.DemoApp.ViewModels;
using OxyPlotEx.DemoApp;
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

            plotView.Model = new OxyPlot.PlotModel();
            var model1 = new MultiDateTimeModel<string>(plotView.Model, scheduler:ReactiveUI.RxApp.MainThreadScheduler) { };
            DataSource.Observe().Take(1000).Subscribe(model1);

            plotView2.Model = new OxyPlot.PlotModel();
            var model2 = new MultiDateTimeModel<string>(plotView2.Model, scheduler: ReactiveUI.RxApp.MainThreadScheduler) { };



            var obs = DataSource.Observe2();

            obs.Subscribe(model2);
            obs
                  .ToObservableChangeSet()
                .Bind(out var collection)
                .Subscribe();
               


            model2.Subscribe(p =>
            {
                var n = collection.Select((a, i) => (a.Value.Item1, i)).Single(a => a.Item1 == p.DateTime).i;
                DataGrid1.SelectedIndex = n;
                DataGrid1.ScrollIntoView(DataGrid1.Items[n]);
            });

            DataGrid1.ItemsSource = collection;

            var obs2 = DataSource.Observe().Select(a => (KeyValuePair<string, (DateTime, double)>?)a).Delay(TimeSpan.FromSeconds(5)).StartWith(default(KeyValuePair<string, (DateTime, double)>?));
            ViewModelViewHost1.ViewModel = new BusyViewModel(obs2);
        }
    }
}

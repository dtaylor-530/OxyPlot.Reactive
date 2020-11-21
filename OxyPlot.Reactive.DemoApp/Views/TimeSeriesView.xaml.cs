using DynamicData;
using MoreLinq;
using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.ViewModels;
using ReactivePlot.Base;
using ReactivePlot.Data.Factory;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using ReactivePlot.Data.Common;
using ReactivePlot.Common;
using ReactivePlot.OxyPlot;
using ReactivePlot.OxyPlot.PlotModel;

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
            var pacedObs = TimeDataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.1));

            pacedObs.SubscribeCustom(new OxyTimeModel<string>(plotView.Model ??= new OxyPlot.PlotModel(), scheduler: ReactiveUI.RxApp.MainThreadScheduler) { });

            var observable = new OxyTimePlotModel<string>(plotView2.Model ??= new OxyPlot.PlotModel());

            var model2 = new TimeModel<string>(observable, scheduler: ReactiveUI.RxApp.MainThreadScheduler) { };

            var obs = TimeDataSource.Observe20();
            obs.SubscribeCustom(model2);
            obs
                .ToObservableChangeSet()
                .Bind(out var collection)
                .Subscribe();

            observable.Subscribe(p =>
            {
                var n = collection.Index().Single(a => a.Value.Value.Key == p.Var).Key;
                DataGrid1.SelectedIndex = n;
                DataGrid1.ScrollIntoView(DataGrid1.Items[n]);
            });

            DataGrid1.ItemsSource = collection;

            var obs2 = pacedObs.Select(a => (KeyValuePair<string, KeyValuePair<DateTime, double>>?)a).Delay(TimeSpan.FromSeconds(5)).StartWith(default(KeyValuePair<string, KeyValuePair<DateTime, double>>?));
            ViewModelViewHost1.ViewModel = new BusyViewModel(obs2);

            var timeModel = new OxyTimeModel<string>(plotView1.Model ??= new OxyPlot.PlotModel(), scheduler: ReactiveUI.RxApp.MainThreadScheduler) { };

            TimeDataSource.Observe1000().Concat(TimeDataSource.Observe1000())
                .SubscribeCustom(timeModel);

            AllToggleButton.SelectToggleChanges().Subscribe(timeModel.OnNext);
        }
    }
}
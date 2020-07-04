using DynamicData;
using Exceptionless.DateTimeExtensions;
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
    public partial class MultiDateTimeGroupView : UserControl
    {
        MultiDateTimeGroupModel<string> model;
        public MultiDateTimeGroupView()
        {
            InitializeComponent();
            var pacedObs = DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.1));

            model = new MultiDateTimeGroupModel<string>(PlotView1.Model ??= new OxyPlot.PlotModel(), scheduler: ReactiveUI.RxApp.MainThreadScheduler);
            pacedObs.Subscribe(model);

            model.OnNext(TimeSpan.FromDays(1));
        }

        private void IntervalBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = TimeUnit.Parse(NumbersBox.SelectedItem.ToString() + ((IntervalBox?.SelectedItem.ToString())?.First().ToString().ToLower() ?? "s"));
            model?.OnNext(x);
        }
    }
}

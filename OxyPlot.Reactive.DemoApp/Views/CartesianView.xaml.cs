using ReactivePlot.Base;
using ReactivePlot.Data.Factory;
using ReactiveUI;
using System;
using System.Windows.Controls;
using ReactivePlot.Data.Common;
using ReactivePlot.Common;
using ReactivePlot.OxyPlot;
using ReactivePlot.Ex;
using System.Reactive.Linq;
using System.Threading;
using ReactivePlot.Model;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for CartesianView.xaml
    /// </summary>
    public partial class CartesianView : UserControl
    {
        public CartesianView()
        {
            InitializeComponent();

            var model = new OxyCartesianModel<string>(PlotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            var model2 = new CartesianModel<string>(new DataGridPlotModel<IDoublePoint<string>>(DataGrid1), scheduler: RxApp.MainThreadScheduler);

            var model3 = new CartesianModel(new SummaryPlotModel(SummaryListBox1), scheduler: RxApp.MainThreadScheduler);


            var obs = DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.1))
                .ToDoubles()
                .Publish()
                .RefCount();


            obs.SubscribeCustom(model);

            obs.SubscribeCustom(model2);

            obs.SubscribeCustom(model3);
        }
    }
}
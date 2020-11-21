using ReactivePlot.Base;
using ReactivePlot.Data.Factory;
using ReactiveUI;
using System;
using System.Windows.Controls;
using ReactivePlot.Data.Common;
using ReactivePlot.Common;
using ReactivePlot.OxyPlot;

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

            DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.1))
                .ToDoubles()
                .SubscribeCustom(model);
        }
    }
}
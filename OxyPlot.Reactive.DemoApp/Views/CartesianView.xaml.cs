using OxyPlot.Data.Common;
using OxyPlot.Data.Factory;
using ReactiveUI;
using System;
using System.Windows.Controls;

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

            var model = new CartesianModel<string>(PlotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            DataSource.Observe1000().Pace(TimeSpan.FromSeconds(0.1))
                .ToDoubles()
                .Subscribe(model);
        }
    }
}
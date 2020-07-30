using OxyPlot.Reactive.DemoApp.Common;
using OxyPlot.Reactive.DemoApp.Factory;
using OxyPlot.Reactive.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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
                // .Select(a=> new OxyPlot.Reactive.Model.Point<string>(a.Value.Key, a.Value.Value,a.Key) as IPoint<string>)
                .Subscribe(model);

        }
    }
}

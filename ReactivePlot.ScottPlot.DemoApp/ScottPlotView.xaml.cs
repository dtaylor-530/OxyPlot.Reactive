using ReactivePlot.Base;
using ReactivePlot.Data.Factory;
using ReactivePlot.ScottPlot;
using System;
using System.Windows.Controls;
using ReactivePlot.Data.Common;
using System.Reactive.Linq;
using ScottPlot;
using ReactivePlot.Model;
using System.Threading;
using System.Reactive.Concurrency;

namespace ReactivePlot.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for ScottPlotView.xaml
    /// </summary>
    public partial class ScottPlotView : UserControl
    {
        public ScottPlotView()
        {
            InitializeComponent();

            One();
            Two();

            void One()
            {
                var plot = new WpfPlot();
                Grid1.Children.Add(plot);

                var dis = DataSource
                    .Observe1000()
                    .Pace(TimeSpan.FromSeconds(0.5))
                    .Select(a => a.Value.Value)           
                    .Subscribe(new ScottSingleSeriesModel(plot));
            }

            void Two()
            {
                var plot = new WpfPlot();
                Grid2.Children.Add(plot);
               

                var dis = DataSource
                    .Observe1000XYPlusMinus()
                    .Pace(TimeSpan.FromSeconds(0.5))
                    .Select(a => (a.Value.Key, a.Value.Value))
                    .Subscribe(new ScottSingleSeries2Model(plot));
            }
        }
    }


    class ScottSingleSeriesModel : SingleSeriesModel
    {
        public ScottSingleSeriesModel(WpfPlot plot) :
            base(new DoubleModel(plot), SynchronizationContext.Current, default)
        {
        }
    }

    class ScottSingleSeries2Model : SingleSeriesModel<(double x, double y)>
    {
        public ScottSingleSeries2Model(WpfPlot plot) : 
            base(new StatisticsModel(plot), SynchronizationContext.Current, default)
        {
        }
    }
}

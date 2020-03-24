using OxyPlotEx.DemoApp;
using OxyPlotEx.ViewModel;
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

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for LineView.xaml
    /// </summary>
    public partial class LineView : UserControl
    {
        public LineView()
        {
            InitializeComponent();

            plotView.Model = new OxyPlot.PlotModel();
            var model1 = new MultiLineModel<string>(new DispatcherX(this.Dispatcher), plotView.Model) { };
            //var model2 = new LineModel<string>(new DispatcherX(this.Dispatcher), plotView.Model) { };
            ProduceData().Subscribe(model1);

        }

        private static IObservable<KeyValuePair<string, (DateTime, double)>> ProduceData()
        {
            DateTime now = DateTime.Now;
            var get2 = new DataFactory().GetLine().GetEnumerator();

            var observable1 = Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(t =>
            {
                get2.MoveNext();
                return get2.Current;
            }).Skip(1);

            var obs1 = observable1.Select((o, i) =>
            {
                return new KeyValuePair<string, (DateTime, double)>(o.Key, (now.AddHours(i), o.Value));
            });
            return obs1;
        }

        private void Multi_Line_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

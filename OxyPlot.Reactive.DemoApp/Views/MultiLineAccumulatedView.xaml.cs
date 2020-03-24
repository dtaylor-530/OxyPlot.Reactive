using OxyPlotEx.DemoApp;
using OxyPlotEx.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for MultiLineAccumulatedView.xaml
    /// </summary>
    public partial class MultiLineAccumulatedView
    {
        private readonly MultiLineModelAccumulated<string> model2;
        private readonly IDisposable disposable;
        Random random = new Random();
        DateTime now = DateTime.Now;

        public MultiLineAccumulatedView()
        {
            InitializeComponent();

            plotView2.Model = new OxyPlot.PlotModel();
            model2 = new MultiLineModelAccumulated<string>(new DispatcherX(this.Dispatcher), plotView2.Model) { ShowAll = true };
            ProduceData(out var observable1, out var observable2);
         
            //var obs0 = observable1.Select((o, i) => new KeyValuePair<string, double>(o.Key, o.Value));
            //var obs1 = observable1.Select((o, i) => new KeyValuePair<string, (DateTime, double)>(o.Key, (now.AddHours(i), o.Value)));
            var obs2 = observable2.Select((o, i) => new KeyValuePair<string, (DateTime, double)>(o.Key, (now.AddHours(i), o.Value)));
            //var obs3 = observable1.Select((o, i) => new KeyValuePair<string, (DateTime, double, double)>(string.Empty, (now.AddHours(i), o.Value, random.NextDouble())));

            //obs0.Subscribe(model1);      
            disposable = obs2.Subscribe(model2);
            //obs1.Subscribe(model3);

            //obs3.Subscribe(model4);

        }

        private static void ProduceData(out IObservable<KeyValuePair<string, double>> observable1, out IObservable<KeyValuePair<string, double>> observable2)
        {
            var get = new DataFactory().GetSin().GetEnumerator();
            observable1 = Observable.Interval(TimeSpan.FromMilliseconds(0.5)).Select(t => { get.MoveNext(); return get.Current; }).Skip(1);
            var get2 = new DataFactory().GetLine().GetEnumerator();
            observable2 = Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(t =>
            {
                get2.MoveNext();
                return get2.Current;
            }).Skip(1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }
    }
}

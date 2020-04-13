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
    /// Interaction logic for MultiDateTimeModelAccumulatedView.xaml
    /// </summary>
    public partial class MultiDateTimeModelAccumulatedView
    {
        private readonly MultiDateTimeAccumulatedModel<string> model, model2;
        private readonly IDisposable disposable;
        DateTime now = DateTime.Now;

        public MultiDateTimeModelAccumulatedView()
        {
            InitializeComponent();

            plotView.Model = new OxyPlot.PlotModel();
            model = new MultiDateTimeAccumulatedModel<string>(new DispatcherX(this.Dispatcher), plotView.Model);
            model.OnNext(true);
            ProduceData(out var observable1, out var observable2);
            var obs = observable2.Select((o, i) => new KeyValuePair<string, (DateTime, double)>(o.Key, (now.AddHours(i), o.Value)));
            disposable = obs.Subscribe(model);

            plotView2.Model = new OxyPlot.PlotModel();
            model2 = new MultiDateTimeAccumulatedModel<string>(new DispatcherX(this.Dispatcher), plotView2.Model);
            model2.OnNext(true);
            var obs2 = observable1.Select((o, i) => (IDateTimeKeyPoint<string>)new DateTimePoint(now.AddHours(i), o.Value, o.Key));
            disposable = obs2.Subscribe(model2);
        }

        private static void ProduceData(out IObservable<KeyValuePair<string, double>> observable1, out IObservable<KeyValuePair<string, double>> observable2)
        {
            var get = new DataFactory().GetSin().GetEnumerator();
            observable1 = Observable.Interval(TimeSpan.FromMilliseconds(50)).Select(t => { get.MoveNext(); return get.Current; }).Skip(1);
            var get2 = new DataFactory().GetLine().GetEnumerator();
            observable2 = Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(t =>           {                get2.MoveNext();                return get2.Current;            }).Skip(1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }
    }
}

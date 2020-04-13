using OxyPlotEx.DemoApp;
using OxyPlotEx.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class MultiDateTimeModelView : UserControl
    {
        public MultiDateTimeModelView()
        {
            InitializeComponent();

            plotView.Model = new OxyPlot.PlotModel();
            var model1 = new MultiDateTimeModel<string>(new DispatcherX(this.Dispatcher), plotView.Model) { };
            ProduceData().Subscribe(model1);

            plotView2.Model = new OxyPlot.PlotModel();
            var model2 = new MultiDateTimeModel<string>(new DispatcherX(this.Dispatcher), plotView2.Model) { };

            var array = new DataFactory().GetLine().Skip(100).Take(20).Select((o, i) =>
             {
                 return new KeyValuePair<string, (DateTime, double)>(o.Key, (DateTime.UnixEpoch.AddYears(i), o.Value));
             }).ToArray();
            array.ToObservable().Subscribe(model2);

            model2.Subscribe(p =>
            {
                var n =  array.Select((a,i)=>(a.Value.Item1,i)).Single(a => a.Item1 == p.DateTime).i;
                DataGrid1.SelectedIndex = n;
                DataGrid1.ScrollIntoView(DataGrid1.Items[n]);
            });

            DataGrid1.ItemsSource = array;


            plotView3.Model = new OxyPlot.PlotModel();
            var model3 = new MultiDateTimeModel<string>(new DispatcherX(this.Dispatcher), plotView3.Model) { };
            Observable.Return(10).Subscribe(model3);
            ProduceData().Delay(TimeSpan.FromSeconds(15)).Subscribe(model3);
            Observable.Timer(TimeSpan.FromSeconds(15)).Select(a=>false).StartWith(true)
                .SubscribeOnDispatcher()
                .Subscribe(a =>
            {
                this.Dispatcher.InvokeAsync(()=> ProgressRingContentControl1.IsBusy = a);
            });

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

      
    }
}

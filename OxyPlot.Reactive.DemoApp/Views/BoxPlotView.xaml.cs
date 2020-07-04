using OxyPlot.Reactive;
using OxyPlotEx.DemoApp;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace OxyPlotEx.DemoAppCore
{
    /// <summary>
    /// Interaction logic for BoxPlotView.xaml
    /// </summary>
    public partial class BoxPlotView : UserControl
    {
        public BoxPlotView()
        {
            InitializeComponent();


            _ = GenerateData().Subscribe(new MultiBoxPlotModel(PlotView1.Model ??= new OxyPlot.PlotModel()));

        }

        private static IObservable<KeyValuePair<string, KeyValuePair<int, double>>> GenerateData()
        {
            DateTime now = DateTime.Now;
            var get2 = new DataFactory().GetLine().GetEnumerator();
            Random random = new Random();
            var observable1 = Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(t =>
            {
                get2.MoveNext();
                return get2.Current;
            }).Skip(1);

            var obs1 = observable1.Select(o =>
            {
                return new KeyValuePair<string, KeyValuePair<int, double>>(o.Key, KeyValuePair.Create(random.Next(0, 10), o.Value));
            });
            return obs1;
        }

    }
}

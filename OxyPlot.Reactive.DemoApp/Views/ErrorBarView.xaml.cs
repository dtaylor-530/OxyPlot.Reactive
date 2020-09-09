using MoreLinq;
using OxyPlot.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace OxyPlotEx.DemoAppCore
{
    /// <summary>
    /// Interaction logic for ErrorBarView.xaml
    /// </summary>
    public partial class ErrorBarView : UserControl
    {
        private static readonly string abc = "abcdefghij";

        public ErrorBarView()
        {
            InitializeComponent();

            _ = GenerateData().Subscribe(new ErrorBarModel(PlotView1.Model ??= new OxyPlot.PlotModel()));
        }

        private static IObservable<KeyValuePair<string, double>> GenerateData()
        {
            Random random = new Random();

            return Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(o =>
            {
                return new KeyValuePair<string, double>(abc[random.Next(0, 10)].ToString(), random.Next(-10, 10));
            });
        }

        private static IObservable<KeyValuePair<string, double>> GenerateData2()

            => Enumerable.Repeat(1, 100).Scan(default(KeyValuePair<string, double>), (a, b) => KeyValuePair.Create("aa", 1d * b)).ToObservable();
    }
}
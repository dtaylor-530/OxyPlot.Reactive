using MoreLinq;
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

namespace OxyPlotEx.DemoAppCore
{
    /// <summary>
    /// Interaction logic for ErrorBarView.xaml
    /// </summary>
    public partial class ErrorBarView : UserControl
    {
        static readonly string abc = "abcdefghij";

        public ErrorBarView()
        {
            InitializeComponent();

            PlotView1.Model = new OxyPlot.PlotModel();
            var bModel = new ErrorBarModel(Constants.DispatcherX, PlotView1.Model);

            _ = GenerateData().Subscribe(bModel);

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

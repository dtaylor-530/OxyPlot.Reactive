using OxyPlot.Reactive;
using OxyPlotEx.DemoApp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// Interaction logic for HeatMapView.xaml
    /// </summary>
    public partial class HeatMapView : UserControl
    {
        public HeatMapView()
        {
            InitializeComponent();

            var hmapModel = new HeatMap(new WDComparer(), null, "WeekDay", "Number", scheduler:ReactiveUI.RxApp.MainThreadScheduler);
            plotView.Model = hmapModel.Model;
            _ = NewMethod().Subscribe(hmapModel);


            //var hmapModel2 = new HeatMap(new WDComparer(), null);
            //plotView2.Model = hmapModel2.Model;
            //_ = NewMethod2(out _).Subscribe(hmapModel2);
        }

        private IObservable<KeyValuePair<(string, string), double>> NewMethod()
        {
            Random rand = new Random();
            string[] dowArray;
            dowArray = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().Select(a => a.ToString()).ToArray();
            var numbers = Enumerable.Range(0, 10).Select(a => a.ToString()).ToArray();
            rand = new Random();
            return Enumerable.Range(0, 300)
                            .Select(a => ((dowArray[rand.Next(0, dowArray.Length)], numbers[rand.Next(0, numbers.Length)]), rand.NextDouble()))
                            .GroupBy(a => a.Item1)
                            .Select(a => (a.Key, a.First().Item2))
                            .ToDictionary(a => a.Key, a => a.Item2).ToObservable();


        }
        private IObservable<KeyValuePair<(string, string), double>> NewMethod2(out IDisposable disp)
        {
            Random rand = new Random();
            string[] dowArray;
            dowArray = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().Select(a => a.ToString()).ToArray();
            var numbers = Enumerable.Range(0, 10).Select(a => a.ToString()).ToArray();
            var enm = Enumerable.Range(0, 300)
                  .Select(a => ((dowArray[rand.Next(0, dowArray.Length)], numbers[rand.Next(0, numbers.Length)]), rand.Next(-70, 2000) * 1d))
                  .GroupBy(a => a.Item1)
                  .Select(a => (a.Key, a.First().Item2))
                  .ToDictionary(a => a.Key, a => a.Item2).GetEnumerator();
            var obs2 = Observable.Interval(TimeSpan.FromMilliseconds(200))
         .TakeWhile(a => enm.MoveNext())
         .Select(t => enm.Current)
         .Skip(1);
            disp = enm;
            return obs2;
        }


        class WDComparer : IComparer<string>
        {
            public int Compare([AllowNull] string x, [AllowNull] string y)
            {
                var xd = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x);
                var yd = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), y);

                return xd - yd;
            }
        }
    }
}

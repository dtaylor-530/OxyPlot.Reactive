using OxyPlot.Reactive;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

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

            _ = NewMethod().Subscribe(new HeatMap(plotView1.Model ??= new OxyPlot.PlotModel(), hNamesComparer: new WDComparer(), hAxisKey: "WeekDay", vAxisKey: "Number", scheduler: ReactiveUI.RxApp.MainThreadScheduler));

            _ = NewMethod2(out _).Subscribe(new HeatMap(plotView2.Model ??= new OxyPlot.PlotModel(), hNamesComparer: new WDComparer()));
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
            var obs2 = Observable.Interval(TimeSpan.FromMilliseconds(600))
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

using Endless.Functional;
using OxyPlot.Reactive;
using OxyPlot.Reactive.Model;
using OxyPlotEx.DemoApp;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for MultiDateTimeModelAccumulatedView.xaml
    /// </summary>
    public partial class MultiDateTimeModelAccumulatedView
    {
        private readonly IDisposable disposable;


        public MultiDateTimeModelAccumulatedView()
        {
            InitializeComponent();
            ProduceData(out var observable1, out var observable2);


            disposable = observable2.Subscribe(
                new MultiDateTimeAccumulatedModel<string>(plotView.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler)
                .Pipe(a => { a.OnNext(true); return a; }));


            var model2 = new MultiDateTimeAccumulatedModel<string>(plotView2.Model ??= new OxyPlot.PlotModel(), scheduler: RxApp.MainThreadScheduler);
            model2.OnNext(true);

            disposable = observable1.Subscribe(model2);
        }


        private static void ProduceData(out IObservable<IDateTimeKeyPoint<string>> observable1, out IObservable<KeyValuePair<string, (DateTime, double)>> observable2)
        {
            DateTime now = DateTime.Now;
            var get = new DataFactory().GetSin().GetEnumerator();
            observable1 = Observable.Interval(TimeSpan.FromMilliseconds(50)).Select(t => { get.MoveNext(); return get.Current; }).Skip(1)
                .Select((o, i) => (IDateTimeKeyPoint<string>)new DateTimePoint(now.AddHours(i), o.Value, o.Key));
            var get2 = new DataFactory().GetLine().GetEnumerator();
            observable2 = Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(t => { get2.MoveNext(); return get2.Current; }).Skip(1)
                .Select((o, i) => new KeyValuePair<string, (DateTime, double)>(o.Key, (now.AddHours(i), o.Value)));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            disposable.Dispose();
        }
    }
}

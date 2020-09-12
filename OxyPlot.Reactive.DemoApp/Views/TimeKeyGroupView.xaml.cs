using OxyPlot.Data.Factory;
using ReactiveUI;
using System;
using System.Windows.Controls;
using OxyPlot.Data.Common;
using OxyPlot.Reactive;
using System.Collections.Generic;
using OxyPlot.Reactive.Model;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive.Subjects;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for TimeKeyGroupView.xaml
    /// </summary>
    public partial class TimeKeyGroupView : UserControl
    {

        private Subject<double> subject = new Subject<double>();

        public TimeKeyGroupView()
        {
            InitializeComponent();

            ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;

            var dis = new DataFactory().GetLineX()
                .Take(200)
                .Select(x => KeyValuePair.Create(string.Empty,
                KeyValuePair.Create(DateTime.UnixEpoch.AddDays(x.Key), x.Value * 100)));

            var pacedObs = dis.ToObservable().Take(100).Merge(dis.ToObservable().Skip(100).Pace(TimeSpan.FromSeconds(2)));


            var model1 = new TimeKeyDoubleGroupModel<string>(PlotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            subject.Subscribe(model1.OnNext);

            pacedObs.Subscribe(model1, ()=> string.Empty);

            //-------------------

            var model2 = new TimeKeyValueGroupModel(PlotView2.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            subject.Subscribe(model2.OnNext);

            pacedObs.Subscribe(model2);
        }

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.AddedItems.Cast<int>().Select(a => a * 1d).ToObservable().Subscribe(subject.OnNext);
        }
    }

    public static class ObservableExtension
    {
        static readonly Random random = new Random();

        public static IDisposable Subscribe(this IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> observable, TimeKeyValueGroupModel model)
        {
            return observable
                .Select(a => KeyValuePair.Create(a.Key, (ITimePoint<double>)new TimePoint<double>(a.Value.Key, a.Value.Value, random.NextDouble() * 100)))
                .Subscribe(a =>
                model.OnNext(a));
        }
    }
}

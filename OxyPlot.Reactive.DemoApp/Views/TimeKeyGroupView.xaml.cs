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

            var df = new DataFactory().GetLineX()
                .Take(200)
                .Select(x => KeyValuePair.Create(string.Empty,
                KeyValuePair.Create(DateTime.UnixEpoch.AddDays(x.Key), x.Value * 100)));

            var pacedObs = df.ToObservable().Take(100).Merge(df.ToObservable().Skip(100).Pace(TimeSpan.FromSeconds(2)));

            var model1 = new TimeKeyDoubleGroupModel(PlotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            subject.Subscribe(model1.OnNext);

            pacedObs.Subscribe(model1);

            ObservableExtension.Subscribe(pacedObs, model1);
        }

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.AddedItems.Cast<int>().Select(a => a * 1d).ToObservable().Subscribe(subject.OnNext);
        }
    }

    public static class ObservableExtension
    {

        public static IDisposable Subscribe(this IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> observable, TimeKeyDoubleGroupModel model)
        {
            return observable
                .Select(a => KeyValuePair.Create(a.Key, (ITimePoint<string>)new TimePoint<string>(a.Value.Key, a.Value.Value, a.Key)))
                .Subscribe(a =>
                model.OnNext(a));
        }
    }
}

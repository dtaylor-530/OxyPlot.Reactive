using OxyPlot.Reactive.DemoApp.Common;
using ReactivePlot.Data.Factory;
using ReactivePlot.Time;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using ReactivePlot.Data.Common;
using ReactivePlot.Model;
using ReactivePlot.Common;
using ReactivePlot.OxyPlot;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for TimeKeyGroupView.xaml
    /// </summary>
    public partial class TimeKeyGroupView : UserControl
    {
        public TimeKeyGroupView()
        {
            InitializeComponent();

            var dis = new DataFactory().GetLineX()
                .Take(200)
                .Select(x => KeyValuePair.Create(string.Empty,
                KeyValuePair.Create(DateTime.UnixEpoch.AddDays(x.Key), x.Value * 100)));

            var pacedObs = dis.ToObservable().Take(100).Merge(dis.ToObservable().Skip(100).Pace(TimeSpan.FromSeconds(2)));

            var model1 = new OxyTimeLogGroupValueModel<string>(PlotView1.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            pacedObs.SubscribeCustom(model1, () => string.Empty);

            //-------------------

            var dis2 = new DataFactory().GetSin(0.1)
      .Take(200)
      .Select((x, i) => KeyValuePair.Create(string.Empty,
      KeyValuePair.Create(DateTime.UnixEpoch.AddDays(i), x.Value * 100)));


            var model2 = new OxyTimeLogGroupKeyModel(PlotView2.Model ??= new PlotModel(), scheduler: RxApp.MainThreadScheduler);

            dis2.ToObservable().Subscribe(model2);


            AllToggleButton.SelectToggleChanges().Subscribe(model1.OnNext);
            AllToggleButton.SelectToggleChanges().Subscribe(model2.OnNext);
            ComboBox1.SelectItemChanges<double>().Subscribe(model1.OnNext);
            ComboBox1.SelectItemChanges<double>().Subscribe(model2.OnNext);
        }
    }

    public static class ObservableExtension
    {
        static readonly Random random = new Random();

        public static IDisposable Subscribe(this IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> observable, TimeLogGroupKeyModel model)
        {
            return observable
                .Select(a => KeyValuePair.Create(a.Key, (ITimePoint<double>)new TimePoint<double>(a.Value.Key, a.Value.Value, a.Value.Value)))
                .Subscribe(a =>
                model.OnNext(a));
        }
    }
}

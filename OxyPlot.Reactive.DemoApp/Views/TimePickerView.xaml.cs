using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Controls;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for TimeUserControl.xaml
    /// </summary>
    public partial class TimePickerView : UserControl
    {
        private readonly ReplaySubject<TimeSpan> timeSpanChanges = new ReplaySubject<TimeSpan>();

        public TimePickerView()
        {
            InitializeComponent();
        }

        public IObservable<TimeSpan> TimeSpanObservable => timeSpanChanges.AsObservable();

        private void IntervalBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ssx = (IntervalBox?.SelectedItem.ToString())?.First().ToString().ToLower() ?? "s";
            var sw = NumbersBox?.SelectedItem.ToString() is { } s ? int.Parse(s) : 1;
            var x = ssx switch
            {
                "s" => TimeSpan.FromSeconds(sw),
                "m" => TimeSpan.FromMinutes(sw),
                "h" => TimeSpan.FromHours(sw),
                "d" => TimeSpan.FromDays(sw),
                _ => throw new NotImplementedException(),
            };

            timeSpanChanges.OnNext(x);
        }
    }
}

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
        private readonly ReplaySubject<TimeSpan> timeSpanChanges = new ReplaySubject<TimeSpan>(1);

        public TimePickerView()
        {
            InitializeComponent();
            TimeSpanObservable = timeSpanChanges.StartWith(GetTimeSpan()).AsObservable();
        }

        public IObservable<TimeSpan> TimeSpanObservable { get; }

        private void IntervalBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            timeSpanChanges.OnNext(GetTimeSpan());
        }

        private TimeSpan GetTimeSpan()
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
            return x;
        }
    }
}
using Exceptionless.DateTimeExtensions;
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
            var x = TimeUnit.Parse(NumbersBox.SelectedItem.ToString() + ((IntervalBox?.SelectedItem.ToString())?.First().ToString().ToLower() ?? "s"));
            timeSpanChanges.OnNext(x);
        }
    }
}

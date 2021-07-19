using LiveCharts;
using LiveCharts.Configurations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace UtilityWpf.ViewModel.Livecharts
{
    using System.Collections;

    public class TimeChartViewModel
    {
        public SeriesCollection SeriesCollection { get; private set; }

        public Func<double, string> Formatter { get; }
        public double EventDate { get; set; }

        private IDisposable disposable;

        public TimeChartViewModel(IObservable<KeyValuePair<DateTime, double>> observable, string name, IScheduler scheduler) : this()
        {
            disposable = observable
                 .ObserveOn(scheduler)
                 .Subscribe(_ =>
                 {
                     SeriesCollection.GetLineOrNew(name)
                     .Values.Add(new DateModel
                     {
                         DateTime = _.Key,
                         Value = _.Value,
                     });
                 }
                , ex =>
                Console.WriteLine("Error in graph view model"));//.Dispose();
        }

        public TimeChartViewModel(IEnumerable<KeyValuePair<DateTime, double>> series, string name, System.Windows.Threading.Dispatcher dispatcher) : this()
        {
            dispatcher.Invoke(() =>
            {
                var line = SeriesCollection.GetLineOrNew(name);
                foreach (var _ in series)
                    line.Values.Add(new DateModel
                    {
                        DateTime = _.Key,
                        Value = _.Value,
                    });
            });
        }

        public TimeChartViewModel(IObservable<KeyValuePair<object, KeyValuePair<DateTime, double>>> observable, IScheduler scheduler) : this()
        {
            disposable = observable
                .ObserveOn(scheduler)
                .Subscribe(_ =>
                {
                    SeriesCollection.GetLineOrNew(_.Key.ToString())
                    .Values.Add(new DateModel
                    {
                        DateTime = _.Value.Key,
                        Value = _.Value.Value,
                    });
                    //NotifyChanged(nameof(SeriesCollection));
                }
                , ex =>
               Console.WriteLine("Error in graph view model"));//.Dispose();
        }

        public TimeChartViewModel(IEnumerable<KeyValuePair<object, SortedList<DateTime, double>>> lines) : this()
        {
            foreach (var kvp in lines)
                SeriesCollection.AddSeries(kvp.Key.ToString(), kvp.Value);
        }

        public TimeChartViewModel(Dictionary<object, SortedList<DateTime, double>> lines) : this()
        {
            foreach (var kvp in lines)
                SeriesCollection.AddSeries(kvp.Key.ToString(), kvp.Value);
        }

        public TimeChartViewModel(IEnumerable lines) : this()
        {
            //foreach (var kvp in lines)
            //    SeriesCollection.AddSeries(kvp.Key.ToString(), kvp.Value);
        }

        public TimeChartViewModel(Dictionary<string, IEnumerable<Tuple<DateTime, double>>> lines) : this()
        {
            foreach (var kvp in lines)
                SeriesCollection.AddSeries(kvp.Key, kvp.Value.ToList());
        }

        public TimeChartViewModel(IEnumerable<IEnumerable<Tuple<DateTime, double>>> lines) : this()
        {
            foreach (var kvp in lines)
                SeriesCollection.AddSeries("", kvp.ToList());
        }

        public TimeChartViewModel(IObservable<KeyValuePair<string, KeyValuePair<DateTime, Tuple<double, double>>>> measurements, IScheduler scheduler) : this()
        {
            measurements.ObserveOn(scheduler).Subscribe(_ =>
            {
                var line1 = SeriesCollection.GetLineOrNew(_.Key);

                line1.Values.Add(new DateModel
                {
                    DateTime = _.Value.Key,
                    Value = _.Value.Value.Item1,
                });

                line1.Values.Add(new DateModel
                {
                    DateTime = _.Value.Key,
                    Value = _.Value.Value.Item2,
                });
            });
        }

        private TimeChartViewModel()
        {
            var dayConfig = Initialise();

            if (dayConfig != null)
                Formatter = value => (value < 0) ? null : new System.DateTime((long)((value) * TimeSpan.FromHours(1).Ticks)).ToString("t");

            SeriesCollection = new LiveCharts.SeriesCollection(dayConfig);
        }

        private CartesianMapper<DateModel> Initialise()
        {
            return
            LiveCharts.Configurations.Mappers.Xy<DateModel>()
            .X(dayModel => (double)dayModel.DateTime.Ticks / TimeSpan.FromHours(1).Ticks)
            .Y(dayModel => dayModel.Value);
        }

        public void Dispose()
        {
            disposable?.Dispose();
        }

        //public TimeChartViewModel(IObservable<KeyValuePair<DateTime, double>> observable, string name, IScheduler scheduler)

        //{
        //    Initialise();

        //    disposable = observable
        //         .ObserveOn(scheduler)
        //         .Subscribe(_ =>
        //         {
        //             SeriesCollection.GetLineOrNew(name)
        //             .Values.Add(new DateModel
        //             {
        //                 DateTime = _.Key,
        //                 Value = _.Value,
        //             });
        //         }
        //        , ex =>
        //        Console.WriteLine("Error in graph view model"));//.Dispose();
        //}

        //public TimeChartViewModel(IEnumerable<KeyValuePair<DateTime,Tuple< double,double>>> series, string name,string name2, System.Windows.Threading.Dispatcher dispatcher)
        //{
        //    Initialise();

        //    dispatcher.Invoke(() =>
        //    {
        //        var line1 = SeriesCollection.GetLineOrNew(name);
        //        foreach (var _ in series)
        //            line1.Values.Add(new DateModel
        //            {
        //                DateTime = _.Key,
        //                Value = _.Value.Item1,
        //            });

        //        var line2 = SeriesCollection.GetLineOrNew(name2);
        //        foreach (var _ in series)
        //            line2.Values.Add(new DateModel
        //            {
        //                DateTime = _.Key,
        //                Value = _.Value.Item2,
        //            });
        //    });

        //}
        //public TimeChartViewModel(SortedList<DateTime, Tuple<double, double>> series, string name,string name2, System.Windows.Threading.Dispatcher dispatcher)
        //{
        //    Initialise();

        //    dispatcher.Invoke(() =>
        //    {
        //        var line1 = SeriesCollection.GetLineOrNew(name);
        //        foreach (var _ in series)
        //            line1.Values.Add(new DateModel
        //            {
        //                DateTime = _.Key,
        //                Value = _.Value.Item1,
        //            });

        //        var line2 = SeriesCollection.GetLineOrNew(name2);
        //        foreach (var _ in series)
        //            line2.Values.Add(new DateModel
        //            {
        //                DateTime = _.Key,
        //                Value = _.Value.Item2,
        //            });
        //    });

        //}

        //public TimeChartViewModel2(IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> observable, IScheduler scheduler)
        //{
        //    Initialise();
        //    observable.Subscribe(ff =>
        //                Console.WriteLine("price subscription " + ff.Value));

        //    disposable = observable
        //        .ObserveOn(scheduler)
        //        .Subscribe(_ =>
        //        {
        //            SeriesCollection.GetLineOrNew(_.Key)
        //            .Values.Add(
        //                new DateModel
        //                {
        //                    DateTime = _.Value.Key,
        //                    Value = _.Value.Value,
        //                });
        //            //NotifyChanged(nameof(SeriesCollection));
        //        }
        //        , ex =>
        //       Console.WriteLine("Error in graph view model"));//.Dispose();

        //}

        public void AddValue(string title, DateTime dt, double value)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                SeriesCollection.GetLineOrNew(title)
             .Values.Add(new DateModel
             {
                 DateTime = dt,
                 Value = value
             });
            });
        }

        public void AddValues(string title, IEnumerable<Tuple<DateTime, double>> values)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var l = SeriesCollection.GetLineOrNew(title);
                foreach (var val in values)
                    l.Values.Add(new DateModel
                    {
                        DateTime = val.Item1,
                        Value = val.Item2
                    });
            });
        }

        internal void RemoveLines(Func<LiveCharts.Definitions.Series.ISeriesView, bool> p)
        {
            foreach (LiveCharts.Definitions.Series.ISeriesView ls in SeriesCollection)
            {
                if (p(ls))
                    SeriesCollection.Remove(ls);
            }
        }
    }
}
using Betfair.ViewModel.Base;
using DynamicData;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Reactive;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Reactive.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Profit = Betfair.Model.Profit;

namespace Betfair.ViewModel.Profits
{
    public abstract class TestSubChartViewModel : ReactiveObject
    {
        protected TestSubChartViewModel(string title, WagerModifierType wagerModifierType)
        {
            Title = title;
            WagerModifier = wagerModifierType;
        }

        protected abstract PlotModel BuildPlotModel();

        public string Title { get; }
        public WagerModifierType WagerModifier { get; }

        // Can't reuse plotmodel otherwise exception, hence why not cached.
        public PlotModel Model => BuildPlotModel();

        public virtual ReadOnlyObservableCollection<DateTimeProfits> Collection { get; } = new ReadOnlyObservableCollection<DateTimeProfits>(new ObservableCollection<DateTimeProfits>());

        public virtual IDateTimeKeyPoint<string> Selected { get; }

        public ReactiveCommand<Unit, Unit> Back = ReactiveCommand.Create<Unit, Unit>(a => a);
    }


    public class TestSubChart1ViewModel : TestSubChartViewModel
    {
        private readonly Lazy<IObservable<IChangeSet<DateTimeProfits, string>>> dateTimeProfitsObservable;
        private readonly Lazy<ReadOnlyObservableCollection<DateTimeProfits>> collection;
        private readonly ObservableAsPropertyHelper<IDateTimeKeyPoint<string>> selected;

        private readonly Subject<IDateTimeKeyPoint<string>> selectedSubject = new Subject<IDateTimeKeyPoint<string>>();
        public TestSubChart1ViewModel(string title, WagerModifierType wagerModifierType, IObservable<(DateTime dateTime, IEnumerable<Profit> profits)> obs) : base(title, wagerModifierType)
        {
            dateTimeProfitsObservable = new Lazy<IObservable<IChangeSet<DateTimeProfits, string>>>(() =>
            {
                return obs
                .SelectMany(a => sdfdf(a.dateTime, a.profits.ToArray(), wagerModifierType))
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ToObservableChangeSet(a => a.Key);
            });

            collection = new Lazy<ReadOnlyObservableCollection<DateTimeProfits>>(() =>
            {
                dateTimeProfitsObservable.Value
           .Bind(out var coll)
           .Subscribe(a =>
           {
           },
                        ex =>
                        { },
                        () => { });
                return coll;
            });
            selected = selectedSubject.ToProperty(this, a => a.Selected);
        }

        protected override PlotModel BuildPlotModel()
        {
            var model = new PlotModel();

            dateTimeProfitsObservable.Value
                .Subscribe(a =>
                {

                });

            var multiModel = new MultiDateTimeAccumulatedModel<string>((IDispatcher)Splat.Locator.Current.GetService(typeof(IDispatcher)), model);
            multiModel.OnNext(true);

            _ = multiModel.Subscribe(selectedSubject);

            _ = dateTimeProfitsObservable.Value
               .MergeMany(a =>
               Observable.Return((IDateTimeKeyPoint<string>)new DateTimePoint(a.DateTime, a.GetDataPoint().Y, a.Key ?? "Key is null")))
               .Select(a =>
               {
                   return a;
               }).Subscribe(multiModel);



            return model;
        }

        static IEnumerable<DateTimeProfits> sdfdf(DateTime dateTime, IEnumerable<Profit> profits, WagerModifierType wagerModifierType)
        {

            var groupedProfits = profits.GroupBy(a => a.Key).Select(a => new DateTimeProfits(dateTime, a.ToArray(), wagerModifierType, a.Key));
            return groupedProfits;

        }

        public override ReadOnlyObservableCollection<DateTimeProfits> Collection => collection.Value;

        public override IDateTimeKeyPoint<string> Selected => selected.Value;
    }


    [Description("Error")]
    public class TestSubChart2ViewModel : TestSubChartViewModel
    {
        private readonly IObservable<KeyValuePair<string, IEnumerable<Profit>>> obs;

        public TestSubChart2ViewModel(string title, WagerModifierType wagerModifierType, IObservable<KeyValuePair<string, IEnumerable<Profit>>> obs) : base(title, wagerModifierType)
        {
            this.obs = obs;
        }

        protected override PlotModel BuildPlotModel()
        {
            var model = new PlotModel();

            obs.Subscribe(a =>
            {

            });

            _ = obs
                  .Select(a => (a.Key, a.Value.ToAggregateProfits(WagerModifier)))
                  .Select(n =>
                  KeyValuePair.Create(n.Key ?? "Key is null", n.Item2))
           .SubscribeOn(RxApp.TaskpoolScheduler)
           .Subscribe(new ErrorBarModel((IDispatcher)Splat.Locator.Current.GetService(typeof(IDispatcher)), model));
            return model;
        }
    }

    public class DateTimeProfits : IDataPointProvider
    {
        private readonly Lazy<DataPoint> dataPoint;
        private readonly Lazy<string> key;

        public DateTimeProfits(DateTime dateTime, Profit[] profits, WagerModifierType wagerModifierType, string key)
        {
            dataPoint = new Lazy<DataPoint>(() => new DataPoint(DateTimeAxis.ToDouble(dateTime), profits.ToAggregateProfits(wagerModifierType)));
            DateTime = dateTime;
            Profits = profits;
            this.key = new Lazy<string>(() => key);
            //    new Lazy<string>(() =>
            //{
            //    try
            //    {
            //        return profits.GroupBy(a => a.Key).Single().Key;
            //    }
            //    catch (Exception ex) { 
            //        return default; }
            //});
        }

        public DateTime DateTime { get; }

        public Profit[] Profits { get; }

        public string Key => key.Value;

        public DataPoint GetDataPoint() => dataPoint.Value;

        public static (DateTime, decimal) Standard2(IEnumerable<Profit> arr)
        {

            return (arr.First().EventDate, arr.Sum(b => (decimal)b.Amount));
        }

        public static (DateTime, decimal) ByEfficiency2(IEnumerable<Profit> arr)
        {
            return (arr.First().EventDate, arr.Sum(b => b.Amount / (1m * b.Wager)));
        }
    }


    static class Converter
    {
        public static double ToAggregateProfits(this IEnumerable<Profit> profits, WagerModifierType wagerModifierType)
        =>
            wagerModifierType switch
            {
                WagerModifierType.None => (double)Standard(profits),
                WagerModifierType.Price => (double)OverPrice(profits),
                WagerModifierType.Wager => (double)OverWager(profits),
                _ => throw new ArgumentOutOfRangeException($"Unexpected value, {wagerModifierType.ToString()}")
            };


        static decimal Standard(IEnumerable<Profit> a) => a.Sum(b => (decimal)b.Amount);

        static decimal OverWager(IEnumerable<Profit> a) => a.Average(b => b.Amount / (1m * b.Wager));

        static decimal OverPrice(IEnumerable<Profit> a) => a.Sum(b => b.Amount.Amount / (1m * b.Price.Value.Decimal));
    }
}
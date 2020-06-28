
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Endless;
using System.Reactive.Threading.Tasks;
using System.Collections.ObjectModel;
//using ReactiveUI;
//using DynamicData;
//using UtilityInterface.NonGeneric;
//using Betfair.ViewModel.Base;
//using Betfair.Abstract.Enums;
//using Betfair.BLL;
//using Betfair.Model;
using System.Reactive;

namespace Betfair.ViewModel.Profits
{
    public class SubViewModel : ReactiveObject, IName
    {
        //private readonly ITestModel testModel;
        //private SubChartViewModel selected;

        //public TestSubViewModel(ITestModel testModel, string name, WagerModifierType wagerModifierType = default)
        //{
        //    Name = name;
        //    WagerModifierType = wagerModifierType;
        //    this.testModel = testModel;
        //}

        //private ReadOnlyObservableCollection<SubChartViewModel> CreateCollection(WagerModifierType wagerModifierType)
        //{

        //    CreateCollection(CreateObservable(TimeInterval, testModel), out var collection, wagerModifierType);

        //    return collection;


        //    static void CreateCollection(IObservable<(string key, IObservable<(DateTime, IEnumerable<Profit>)> obs)> obs, out ReadOnlyObservableCollection<SubChartViewModel> collection, WagerModifierType wagerModifierType) =>
        //        obs
        //            .Select(a => new TestSubChart1ViewModel(a.key, wagerModifierType, a.obs))
        //            .Cast<SubChartViewModel>()
        //            .StartWith(new TestSubChart2ViewModel("Error", wagerModifierType, obs.SelectMany(a => a.obs.Select(b =>
        //                KeyValuePair.Create(a.key + b.Item2.FirstOrDefault().Key == null ? string.Empty : Environment.NewLine + b.Item2.First().Key, b.Item2)))))
        //            .ToObservableChangeSet()
        //            .Bind(out collection)
        //            .Subscribe();


        //    static IObservable<(string key, IObservable<(DateTime, IEnumerable<Model.Profit>)> obs)> CreateObservable(TimeInterval timeInterval, ITestModel testModel) =>
        //        testModel
        //            .SelectAll()
        //            .Select(a => (a.Key, obs: ConvertProfit(timeInterval, a.Value)))
        //            .ToObservable()
        //            .SubscribeOn(RxApp.TaskpoolScheduler)
        //            .ObserveOn(RxApp.MainThreadScheduler);


        //    static IObservable<(DateTime, IEnumerable<Model.Profit>)> ConvertProfit(TimeInterval timeInterval, Task<Model.Profit[]> asyncEnumerable) =>
        //        asyncEnumerable
        //            .ToObservable()
        //            .Select(profit => TimeIntervalToGroupFunc(timeInterval).Invoke(profit))
        //            .SubscribeOn(RxApp.TaskpoolScheduler)
        //            .SelectMany(b => b.ToObservable());
        //}


        public ReadOnlyObservableCollection<SubChartViewModel> SubModels { get; }

        public SubChartViewModel Selected { get; set; }

        public UtilityEnum.TimeInterval TimeInterval { get; }

        public string Name { get; }
        //public WagerModifierType WagerModifierType { get; }

        //public Task<(double mean, double variance)> GetStatistics() => testModel.GetStatistics();

        ////GroupProfitByMonth(this IEnumerable<Model.Profit> profit, string name, bool byEfficiency)
        //private static Func<IEnumerable<Model.Profit>, IEnumerable<(DateTime, IEnumerable<Model.Profit>)>> TimeIntervalToGroupFunc(TimeInterval timeInterval) => timeInterval switch
        //{
        //    TimeInterval.Week => ProfitHelper.GroupProfitByWeek,
        //    TimeInterval.Day => ProfitHelper.GroupProfitByDay,
        //    TimeInterval.Month => ProfitHelper.GroupProfitByMonth,
        //    _ => throw new NotSupportedException($"Unsupported {nameof(TimeInterval)}, {timeInterval.ToString()}")
        //};
    }
}

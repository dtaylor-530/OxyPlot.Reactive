using Betfair.Abstract;
using System.Collections.Generic;
using System.Linq;
using Betfair.BLL;
using System.Reactive.Linq;
using Endless;
using DynamicData;
using System.Collections.ObjectModel;
using System;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using UtilityInterface.NonGeneric;
using Betfair.ViewModel.Base;

namespace Betfair.ViewModel.Profits
{

    [Base.ViewModel]
    public class TestViewModel : ReactiveObject, IName
    {
        readonly string name;
        private WagerModifierType wagerModifier = WagerModifierType.None;
        readonly Lazy<ReadOnlyObservableCollection<SubViewModel>> coll;

        //public TestViewModel(ITestMultiModel testModel, string name)
        //{
        //    this.name = name;

        //    coll = new Lazy<ReadOnlyObservableCollection<TestSubViewModel>>(() =>
        //     {
        //         var xx = Enum.GetValues(typeof(WagerModifierType))
        //          .Cast<WagerModifierType>()
        //          .ToObservable()
        //          .SelectMany(a => SelectSubViewModels(a, name, testModel))
        //          .ToObservableChangeSet(a => a.Name)
        //            .Filter(Observable.Return(new Func<TestSubViewModel, bool>(vm =>
        //            WagerModifier == vm.WagerModifierType)), this.WhenAnyValue(a => a.WagerModifier).Select(a =>
        //            Unit.Default))
        //         .ObserveOn(RxApp.MainThreadScheduler)
        //         .Bind(out var _coll)
        //         //.DisposeMany()
        //         .Subscribe(a =>
        //         {

        //         }, e =>
        //          {
        //          });

        //         return _coll;
        //     });


        //    static IObservable<TestSubViewModel> SelectSubViewModels(WagerModifierType modType, string name, ITestMultiModel testMultiModel)
        //    {
        //        return System.Threading.Tasks.Task.Run(() =>
        //        {
        //            try
        //            {
        //                var arr = testMultiModel.Models.ToArray();
        //                var xx = arr.Select(testModel => new TestSubViewModel(testModel, testModel.Name, modType)).ToArray();
        //                return xx;
        //            }
        //            catch (Exception ex)
        //            {
        //                return null;
        //            }
        //        }).ToObservable()
        //            .SubscribeOn(RxApp.MainThreadScheduler)
        //            .ObserveOn(RxApp.MainThreadScheduler)
        //            .SelectMany(a => a.ToObservable());
        //    }
        //}

        public ReadOnlyObservableCollection<SubViewModel> SubViewModels => coll.Value;

        public string Name => name;

        public WagerModifierType WagerModifier { get => wagerModifier; set => this.RaiseAndSetIfChanged(ref wagerModifier, value); }

    }




}

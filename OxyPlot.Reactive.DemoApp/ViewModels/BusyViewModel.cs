using OxyPlot.Reactive.Infrastructure;
using OxyPlotEx.DemoApp;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace OxyPlot.Reactive.DemoApp.ViewModels
{
    public  class BusyViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> isBusy;

        public BusyViewModel(IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>?> observable)
        {

            var model3 = new TimeModel<string>(PlotModel,scheduler: RxApp.MainThreadScheduler) { };

            observable.Where(a => a.HasValue).Select(a => a.Value).Subscribe(model3);

            isBusy = observable.ObserveOnDispatcher().Select(a => !a.HasValue)
                .DistinctUntilChanged()
                .SubscribeOnDispatcher()
               .ToProperty(this, a => a.IsBusy);

        }

        public PlotModel PlotModel { get; } = new PlotModel();


        public bool IsBusy => isBusy.Value;

    }
}

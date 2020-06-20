using OxyPlot.Reactive.DemoApp.ViewModels;
using OxyPlot.Reactive.DemoApp.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Splat.Locator;

namespace OxyPlotEx.DemoAppCore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            CurrentMutable.Register(() => new BusyView(), typeof(IViewFor<BusyViewModel>));

        }
    }
}

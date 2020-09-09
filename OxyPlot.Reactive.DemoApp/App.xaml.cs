using OxyPlot.Reactive.DemoApp.ViewModels;
using OxyPlot.Reactive.DemoApp.Views;
using ReactiveUI;
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
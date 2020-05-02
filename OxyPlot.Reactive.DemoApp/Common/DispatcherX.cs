using OxyPlot.Reactive.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace OxyPlotEx.DemoApp
{
    public static class Constants
    {
        public static readonly IDispatcher DispatcherX = new DispatcherX(Application.Current.Dispatcher);
    }


    public class DispatcherX : IDispatcher
    {
        private Dispatcher dispatcher;

        public DispatcherX(Dispatcher dispatcher) => this.dispatcher = dispatcher;

        public void BeginInvoke(Action action) => dispatcher.BeginInvoke(action);

        public void Invoke(Action action) => dispatcher.Invoke(action);

    }
}

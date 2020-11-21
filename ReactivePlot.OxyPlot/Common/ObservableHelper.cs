using OP = OxyPlot;
using OxyPlot;
using System;
using System.Reactive.Linq;

namespace ReactivePlot.OxyPlot.Common
{
    public static class ObservableHelper
    {
        public static IObservable<OxyMouseDownEventArgs> ToMouseDownEvents(this OP.UIElement uIElement)
        {
            return Observable.FromEventPattern<EventHandler<OxyMouseDownEventArgs>, OxyMouseDownEventArgs>(a => uIElement.MouseDown += a, a => uIElement.MouseDown -= a)
                .Select(a => a.EventArgs);
        }
    }
}
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace OxyPlotEx.Common
{
    public static class ObservableHelper
    {

        public static IObservable<OxyMouseDownEventArgs> ToMouseDownEvents(this UIElement uIElement)
        {
           return Observable.FromEvent<EventHandler<OxyMouseDownEventArgs>, OxyMouseDownEventArgs>(a => uIElement.MouseDown += a, a => uIElement.MouseDown -= a);
        }
    }
}

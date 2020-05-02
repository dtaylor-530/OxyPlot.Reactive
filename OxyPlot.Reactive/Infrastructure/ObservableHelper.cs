using OxyPlot;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace OxyPlot.Reactive.Infrastructure
{
    public static class ObservableHelper
    {

        public static IObservable<OxyMouseDownEventArgs> ToMouseDownEvents(this UIElement uIElement)
        {
            return Observable.FromEventPattern<EventHandler<OxyMouseDownEventArgs>, OxyMouseDownEventArgs>(a => uIElement.MouseDown += a, a => uIElement.MouseDown -= a)
                 .Select(a => a.EventArgs);
        }
    }
}

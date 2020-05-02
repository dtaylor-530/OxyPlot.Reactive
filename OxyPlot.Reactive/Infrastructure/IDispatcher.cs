using System;
using System.Collections.Generic;
using System.Text;

namespace OxyPlot.Reactive.Infrastructure
{
    public interface IDispatcher
    {
        void Invoke(Action action);
        void BeginInvoke(Action action);
    }
}

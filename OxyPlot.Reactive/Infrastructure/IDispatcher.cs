using System;
using System.Collections.Generic;
using System.Text;

namespace OxyPlotEx.ViewModel
{
    public interface IDispatcher
    {
        void Invoke(Action action);
        void BeginInvoke(Action action);
    }
}

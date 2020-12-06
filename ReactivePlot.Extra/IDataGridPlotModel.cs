using ReactivePlot.Model;
using System.Windows.Controls;

namespace ReactivePlot.Ex
{
    public interface IDataGridPlotModel: IPlotModel
    {
        public DataGrid PlotModel { get;  }
    }   
    
    public interface IDataGridPlotModel<T>: IMultiPlotModel<T>
    {
        public DataGrid PlotModel { get;  }
    }
}
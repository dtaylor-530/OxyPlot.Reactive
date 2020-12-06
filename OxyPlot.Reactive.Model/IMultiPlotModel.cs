using System.Collections;
using System.Collections.Generic;

namespace ReactivePlot.Model
{
    public interface ISinglePlotModel<T> : ISinglePlotModel, IAdd<T>
    {
    }
    public interface ISinglePlotModel :  IAdd
    {
        void Clear();

        void Invalidate(bool v);
    }

    public interface IMultiPlotModel<T> : IPlotModel, IAddSeries<T>, IRemoveSeries
    {
    }


    public interface IAdd<T>
    {
        void Add(IReadOnlyCollection<T> items);
    }

    public interface IAdd
    {
        void Add(IEnumerable items);
    }

    public interface IAddSeries<T>
    {
        void AddSeries(IReadOnlyCollection<T> items, string title, int? index = null);

    }


    public interface IRemoveSeries
    {
        bool RemoveSeries(string title);
    }

    public interface IPlotModel
    {
        void Clear();

        void Invalidate(bool v);
    }
}

namespace ReactivePlot.Model
{
    public interface IPlotModel<T> : IPlotModel, IAddData<T>
    {
    }

    public interface IAddData<T>
    {
        void AddData(T[] items, string title, int? index = null);
    }

    public interface IPlotModel
    {
        bool RemoveSeries(string title);

        void ClearSeries();

        void Invalidate(bool v);
    }
}

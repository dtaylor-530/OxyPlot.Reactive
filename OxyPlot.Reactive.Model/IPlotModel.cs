namespace ReactivePlot.Model
{
    public interface IPlotModel<T> : IPlotModel
    {
        void AddToSeries(T[] items, string title, int? index = null);
    }

    public interface IPlotModel
    {
        //void Configure();
        //void ModifyPlotModel();
        bool RemoveSeries(string title);

        void ClearSeries();

        void Invalidate(bool v);
    }
}

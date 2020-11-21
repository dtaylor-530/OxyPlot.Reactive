using OxyPlot;
using OxyPlot.Series;
using ReactivePlot.OxyPlot.Common;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using oxy = OxyPlot;

namespace ReactivePlot.OxyPlot.PlotModel
{
    public abstract class OxyPlotModel<TType3> : OxyBasePlotModel, Model.IPlotModel<TType3>, IObservable<TType3>
    {
        protected readonly Subject<TType3> subject = new Subject<TType3>();

        public OxyPlotModel(oxy.PlotModel plotModel) : base(plotModel)
        {
        }

        public virtual void AddToSeries(TType3[] items, string title, int? index = null)
        {
            var dataPoints = items.Select(Convert);
            lock (PlotModel)
            {
                if (!(PlotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
                {
                    series = OxyFactory.BuildWithMarker(dataPoints, title);


                    if (index.HasValue)
                        PlotModel.Series.Insert(index.Value, series);
                    else
                        PlotModel.Series.Add(series);

                    disposableDictionary.Add(title, series
                        .ToMouseDownEvents()
                        .Select(args => OxyMouseDownAction(args, series, items))
                        .Subscribe(subject.OnNext));

                }
                if (series is LineSeries lSeries)
                {
                    //var count = series.ItemsSource.Count();
                    //lSeries.MarkerSize = (int)(5/ (1 + (Math.Log10(count)))) - 1;
                    //if (count > 100)
                    //    lSeries.MarkerStrokeThickness = 0;
                }

                series.ItemsSource = dataPoints;
            }
        }

        public IDisposable Subscribe(IObserver<TType3> observer)
        {
            return subject.Subscribe(observer);
        }

        protected abstract TType3 OxyMouseDownAction(OxyMouseDownEventArgs e, XYAxisSeries series, TType3[] items);

        protected abstract IDataPointProvider Convert(TType3 item);
    }

    //public abstract class OxyPlotModel<TType3> : OxyPlotModel<TType3>, ReactivePlot.Model.IPlotModel<TType3> where TType3 : IDataPointProvider
    //{
    //    public OxyPlotModel(oxy.PlotModel plotModel) : base(plotModel)
    //    {
    //    }

    //    protected override IDataPointProvider Convert(TType3 item) => item;
    //}
}

using OxyPlot;
using OxyPlot.Series;
using ReactivePlot.Model;
using ReactivePlot.OxyPlot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using oxy = OxyPlot;

namespace ReactivePlot.OxyPlot.PlotModel
{

    public abstract class OxyBasePlotModel : OxyBasePlotModel<DataPointProvider> 
    {
        public OxyBasePlotModel(oxy.PlotModel plotModel) :base(plotModel)
        {
        }
    }
     
    public abstract class OxyBasePlotModel<T> : IOxyPlotModel, IAddSeries<T> 
    {
        protected readonly Dictionary<string, IDisposable> disposableDictionary = new Dictionary<string, IDisposable>();

        public OxyBasePlotModel(oxy.PlotModel plotModel)
        {
            this.PlotModel = plotModel;
            Configure();
        }

        public oxy.PlotModel PlotModel { get; }

        protected virtual void Configure()
        {
            if (PlotModel?.PlotView?.ActualController is IController plotController)
                CongfigureBindings(plotController);

            static void CongfigureBindings(IController pc)
            {
                pc.UnbindMouseDown(OxyMouseButton.Left);
                //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control);
                //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift);

                pc.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                                 (view, controller, args) =>
                                 controller.AddMouseManipulator(view, new TrackerManipulator1(view), args)));
            }
        }


        public virtual void AddSeries(IReadOnlyCollection<T> items, string title, int? index = null)
        {
            lock (PlotModel)
            {
                if (!(PlotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series))
                {
                    series = OxyFactory.BuildWithMarker(items, title);

                    if (index.HasValue)
                        PlotModel.Series.Insert(index.Value, series);
                    else
                        PlotModel.Series.Add(series);

                }
                if (series is LineSeries lSeries)
                {
                    //var count = series.ItemsSource.Count();
                    //lSeries.MarkerSize = (int)(5/ (1 + (Math.Log10(count)))) - 1;
                    //if (count > 100)
                    //    lSeries.MarkerStrokeThickness = 0;
                }

                series.ItemsSource = items;
            }
        }




        public virtual bool RemoveSeries(string title)
        {
            lock (PlotModel)
            {
                //if (index.HasValue)
                //{
                //    plotModel.Series.RemoveAt(index.Value);
                //    return;
                //}

                if (PlotModel.Series.SingleOrDefault(a => a.Title == title) is XYAxisSeries series)
                {
                    disposableDictionary.Remove(title);
                    PlotModel.Series.Remove(series);
                    return true;
                }
                return false;
            }
        }

        public virtual void Clear()
        {
            lock (PlotModel)
            {
                //if (index.HasValue)
                //{
                //    plotModel.Series.RemoveAt(index.Value);
                //    return;
                //}

                PlotModel.Series.Clear();
            }
        }

        public virtual void Invalidate(bool v)
        {
            this.PlotModel.InvalidatePlot(v);
        }
    }
}

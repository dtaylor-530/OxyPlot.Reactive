using ReactivePlot.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ReactivePlot.Ex
{

    //class Constants
    //{
    //    public const string X = "x";
    //}

    public class SummaryPlotModel : ISummaryPlotModel<IDoublePoint>, IAddSeries<IDoublePoint>
    {
        ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        public SummaryPlotModel(SummaryListBox plotModel)
        {
            this.PlotModel = plotModel;

        }

        public SummaryListBox PlotModel { get; }


        public virtual void AddSeries(IReadOnlyCollection<IDoublePoint> items, string title, int? index = null)
        {

            lock (PlotModel)
            {

                actions.Enqueue(new Action(() =>
                {
                    if (!(this.PlotModel.Items.Cast<SummaryControl>().SingleOrDefault(a => a.Key == title) is { } series))
                    {
                        ///SummaryControl series = null;
                        series = new SummaryControl();

                        if (index.HasValue)
                            this.PlotModel.Items.Insert(index.Value, series);
                        else
                            this.PlotModel.Items.Add(series);

                        if (series != null)
                        {
                            series.Key = title;
                            series.ItemsSource = items;
                        }
                    }
                    series.Key = title;
                    series.ItemsSource = items;
                }));
                //disposableDictionary.Add(title, series
                //    .ToMouseDownEvents()
                //    .Select(args => OxyMouseDownAction(args, series, items))
                //    .Subscribe(subject.OnNext));


            }
        }


        public virtual bool RemoveSeries(string title)
        {
            lock (PlotModel)
            {
                if (PlotModel.Items.Cast<SummaryControl>().SingleOrDefault(a => a.Key.ToString() == title) is { } column)
                {
                    PlotModel.Items.Remove(column);
                }

                return false;
            }
        }

        public virtual void Clear()
        {
            lock (PlotModel)
            {
                PlotModel.Items.Clear();
            }
        }



        public virtual void Invalidate(bool v)
        {
            while (actions.TryDequeue(out Action? action) == true)
                action?.Invoke();

        }
    }
}

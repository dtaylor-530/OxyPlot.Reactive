
//namespace OxyPlotEx.ViewModel
//{
//    using MoreLinq;
//    using OxyPlot;
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;

//    public class MultiTimeLineModel<T> : IObserver<IEnumerable<KeyValuePair<T, (DateTime date, double value)>>>, IObserver<KeyValuePair<T, (DateTime date, double value)>>
//    {
//        Dictionary<T, List<DateTimePoint>> DataPoints;

//        private IDispatcher dispatcher;
//        private PlotModel plotModel;
//        object lck = new object();
//        private readonly IEqualityComparer<T> comparer;

//        public MultiTimeLineModel(IDispatcher dispatcher, PlotModel model)
//        {
//            this.dispatcher = dispatcher;
//            this.plotModel = model;
//            model.Axes.Add(new OxyPlot.Axes.DateTimeAxis { });
//            DataPoints = GetDataPoints();
//        }


//        public MultiTimeLineModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : this(dispatcher, model)
//        {
//            this.comparer = comparer;
//        }

//        Dictionary<T, List<DateTimePoint>> GetDataPoints()
//        {
//            return comparer == default ?
//                new Dictionary<T, List<DateTimePoint>>() :
//                new Dictionary<T, List<DateTimePoint>>(comparer);
//        }

//        public bool ShowAll { get; set; } = false;

//        public void OnNext(IEnumerable<KeyValuePair<T, (DateTime date, double value)>> enumerable)
//        {
//            lock (lck)
//            {
//                foreach (var item in enumerable)
//                {
//                    if (!DataPoints.ContainsKey(item.Key))
//                        DataPoints[item.Key] = new List<DateTimePoint>();
//                    var newdp = new DateTimePoint(item.Value.date, item.Value.value);

//                    DataPoints[item.Key].Add(newdp);
//                }

//                Refresh();
//            }
//        }

//        public void OnNext(KeyValuePair<T, (DateTime date, double value)> item)
//        {
//            if(item.Key==null)
//            {

//            }
//            lock (lck)
//            {

//                if (!DataPoints.ContainsKey(item.Key))
//                    DataPoints[item.Key] = new List<DateTimePoint>();
//                var newdp = new DateTimePoint(item.Value.date, item.Value.value);

//                DataPoints[item.Key].Add(newdp);

//                Refresh();
//            }
//        }


//        private void Refresh()
//        {

//            this.dispatcher.BeginInvoke(() =>
//            {
//                lock (lck)
//                {
//                    plotModel.Series.Clear();
//                    foreach (var dataPoint in DataPoints)
//                    {
//                        var points = dataPoint.Value
//                             .OrderBy(dt => dt.DateTime);

//                        plotModel.Series.Add(OxyFactory.Build(points, dataPoint.Key.ToString()));
//                    }

//                    if (ShowAll)
//                    {
//                        var allPoints = DataPoints.SelectMany(a => a.Value)
//                                   .OrderBy(dt => dt.DateTime)
//                                   .GroupBy(a => a.DateTime)
//                                 .Select(xy0 => new DateTimePoint(xy0.Key, xy0.Sum(l => l.Value)));
//                        plotModel.Series.Add(OxyFactory.Build(allPoints, "All"));
//                    }
//                    plotModel.InvalidatePlot(true);
//                }
//            });
//        }

//        public void Reset()
//        {
//            dispatcher.Invoke(() =>
//            {
//                while (plotModel.Series.Any())
//                    plotModel.Series.Remove(plotModel.Series.First());
//                DataPoints = GetDataPoints();
//                plotModel.InvalidatePlot(true);
//            });
//        }

//        public void Remove(ISet<string> names)
//        {
//            dispatcher.Invoke(() =>
//            {
//                foreach(var series in plotModel.Series.Where(s => names.Contains(s.Title)))
//                    plotModel.Series.Remove(series);
//                DataPoints = GetDataPoints();
//                plotModel.InvalidatePlot(true);
//            });
//        }

//        public void OnCompleted()
//        {
//            throw new NotImplementedException();
//        }

//        public void OnError(Exception error)
//        {
//            throw new NotImplementedException();
//        }
//    }


//}


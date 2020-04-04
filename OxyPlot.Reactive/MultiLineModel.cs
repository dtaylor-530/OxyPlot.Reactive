#nullable enable

using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using OxyPlot.Series;
using MoreLinq;

namespace OxyPlotEx.ViewModel
{
    public class MultiLineModel<T> : MultiPlotModel<T, DateTime>, IObservable<DateTimePoint>, IObserver<int>
    {
        readonly List<IDisposable> disposables = new List<IDisposable>();
        readonly Subject<DateTimePoint> subject = new Subject<DateTimePoint>();
        private int? count;

        public MultiLineModel(IDispatcher dispatcher, PlotModel plotModel) : base(dispatcher, plotModel)
        {
        }

        public MultiLineModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : base(dispatcher, model, comparer)
        {
        }

        protected override void ModifyPlotModel()
        {
            plotModel.Axes.Add(new DateTimeAxis());
        }

        protected override void Refresh(IList<Unit> units)
        {
            this.dispatcher.BeginInvoke(async () =>
            {
                foreach (var keyValue in DataPoints.ToArray())
                {
                    _ = await Task.Run(() =>
                      {
                          lock (lck)
                          {
                              return count.HasValue ?
                               Enumerable.TakeLast(ToDataPoints(keyValue.Value), count.Value).ToArray() :
                               ToDataPoints(keyValue.Value).ToArray();
                          }
                      }).ContinueWith(async points =>
                      {
                          AddToSeries(await points, keyValue.Key.ToString());
                      }, TaskScheduler.FromCurrentSynchronizationContext());
                }

                if (ShowAll)
                {
                    _ = await Task.Run(() =>
                     {
                         lock (lck)
                             return count.HasValue ?
                             Enumerable.TakeLast(ToDataPoints(DataPoints.SelectMany(a => a.Value)), count.Value).ToArray() :
                             ToDataPoints(DataPoints.SelectMany(a => a.Value)).ToArray();

                     }).ContinueWith(async points =>
                     {
                         AddToSeries((await points), "All");
                     }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                plotModel.InvalidatePlot(true);
            });

            IEnumerable<DateTimePoint> ToDataPoints(IEnumerable<(DateTime X, double Y)> collection)
                => collection
                        .OrderBy(c => c.X)
                        .Scan(new DateTimePoint(), (xy0, xy) => new DateTimePoint(xy.X, Combine(xy0.Value, xy.Y)))
                        .Skip(1);
        }


        protected virtual void AddToSeries(DateTimePoint[] items, string title)
        {
            if (!(plotModel.Series.SingleOrDefault(a => a.Title == title) is ItemsSeries series))
            {
                series = (OxyFactory.Build(items, title));

                //var observable = series.ToMouseDownEvents().Select(e =>
                //{
                //    var index = e.HitTestResult.Index;
                //    // Index of nearest point in LineSeries
                //    var tt = (int)Math.Round(e.HitTestResult.Index);
                //    var point = items[tt];
                //    return point;
                //});
                //_ = observable.Subscribe(subject);

                series.MouseDown += (s, e) =>
                {
                    var index = e.HitTestResult.Index;
                    // Index of nearest point in LineSeries
                    var tt = (int)Math.Round(e.HitTestResult.Index);
                    var point = items[tt];

                    subject.OnNext(point);
                };

                plotModel.Series.Add(series);

                //return;
            }

            series.ItemsSource = items;
        }

        public IDisposable Subscribe(IObserver<DateTimePoint> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(int count)
        {
            this.count = count;
            refreshSubject.OnNext(Unit.Default);
        }
    }
}



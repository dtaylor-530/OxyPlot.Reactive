#nullable enable

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using OxyPlotModel = OxyPlot.PlotModel;

namespace ReactivePlot.OxyPlot
{
    public class StackedBarModel : IObserver<(string groupkey, string key, double value)>, IObserver<bool>
    {
        private readonly Lazy<OxyPlotModel> model;
        private readonly ReplaySubject<Series> refreshSubject = new ReplaySubject<Series>();
        private readonly ReplaySubject<(string groupkey, string key, double value)> itemSubject = new ReplaySubject<(string groupkey, string key, double value)>();
        private readonly ReplaySubject<bool> stackSubject = new ReplaySubject<bool>();
        private readonly SynchronizationContext scheduler;
        private IDisposable? disposable;
        private readonly object lck = new object();

        public StackedBarModel(SynchronizationContext? scheduler = null)
        {
            this.scheduler = scheduler ?? SynchronizationContext.Current;
            model = new Lazy<OxyPlotModel>(() => PlotBuilder.Build());
            Init();
        }

        private void Init()
        {
            disposable ??= itemSubject.Buffer(TimeSpan.FromMilliseconds(300))
                .Where(a => a.Any())
                .CombineLatest(stackSubject.StartWith(true).DistinctUntilChanged(), (a, b) => (a, b))
                .Select(c => Combine(c.a, c.b))
                .SubscribeOn(scheduler)
                .Subscribe(series =>
                {
                    lock (model)
                        model.Value.InvalidatePlot(true);
                });
        }

        public OxyPlotModel Model => model.Value;

        public void OnCompleted()
        {
            //            disposable.Dispose();
            //Reset();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException($"Error in {nameof(StackedBarModel)}");
            //throw new NotImplementedException();
        }

        public void OnNext(bool value)
        {
            stackSubject.OnNext(value);
        }

        public void OnNext((string groupkey, string key, double value) value)
        {
            itemSubject.OnNext(value);
        }

        public Unit Combine(IList<(string groupkey, string key, double value)> items, bool stack)
        {
            lock (model)
            {
                if (model.Value.DefaultXAxis == null)
                {
                    throw new Exception($"PlotModel needs a {model.Value.DefaultXAxis} (can't be null)");
                }

                var cSeries = EnumerateColumnSeries(stack).ToArray();

                foreach (var gk in items.GroupBy(a => a.groupkey))
                {
                    ColumnSeries s = FindSeries(gk.Key, cSeries, stack);
                    foreach (var (groupkey, key, value) in items)
                    {
                        double val = s.Items.Sum(a => a.Value);
                        double newVal = value;
                        if (groupkey == key)
                        {
                            newVal += val;
                            s.Items.Clear();
                        }

                        s.Items.Add(new ColumnItem(newVal));
                    }
                }
                if (model.Value.DefaultXAxis is CategoryAxis axis)
                {
                    foreach (var k in items.GroupBy(a => (a.groupkey, a.key)).Select(a => a.Key).Where(a => a.groupkey != a.key).Select(a => a.key))
                    {
                        var labels = axis.Labels;
                        if (labels.Contains(k) == false)
                            labels.Add(k);
                    }
                }

                return Unit.Default;
            }
        }

        private int columnCount = 0;

        private ColumnSeries FindSeries(string key, IEnumerable<ColumnSeries> series, bool stack)
        {
            ColumnSeries? cSerie = null;

            foreach (var serie in series)
            {
                if (serie.Title == key)
                {
                    cSerie = serie;
                }
                serie.IsStacked = stack;
            }
            if (cSerie == null)
            {
                cSerie = new ColumnSeries { Title = key, IsStacked = stack };
                columnCount++;
                lock (model)
                    model.Value.Series.Add(cSerie);
            }

            return cSerie;
        }

        private IEnumerable<ColumnSeries> EnumerateColumnSeries(bool stack)
        {
            for (int i = 0; i < columnCount; i++)
            {
                if (model.Value.Series[i] is ColumnSeries cSeries)
                {
                    cSeries.IsStacked = stack;
                    yield return cSeries;
                }
            }
        }

        public void Reset()
        {
            scheduler.Post((a) =>
            {
                lock (model)
                {
                    columnCount = 0;
                    while (model.Value.Series.Any())
                        model.Value.Series.Remove(model.Value.Series.First());

                    if (model.Value.DefaultXAxis is CategoryAxis categoryAxis)
                        categoryAxis.Labels.Clear();
                    model.Value.InvalidatePlot(false);
                }
            }, null);
        }
    }

    public static class PlotBuilder
    {
        public static OxyPlotModel Build()
        {
            (Axis xaxis, Axis yaxis, List<Series> s1) = BuildComponents();

            var model = Construct(xaxis, yaxis, s1);

            return model;

            static OxyPlotModel Construct(Axis xaxis, Axis yaxis, List<Series> s1)
            {
                xaxis.MajorGridlineStyle = LineStyle.Solid;
                xaxis.MinorGridlineStyle = LineStyle.Dot;
                yaxis.MajorGridlineStyle = LineStyle.Dot;
                xaxis.MinorGridlineStyle = LineStyle.Dot;

                var model = new OxyPlotModel();

                model.Axes.Add(xaxis);
                model.Axes.Add(yaxis);
                //foreach (var series in s1)
                //    model.Series.Add(series);

                return model;
            }

            static (Axis, Axis, List<Series>) BuildComponents()
            {
                CategoryAxis xaxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom
                };

                LinearAxis yaxis = new LinearAxis
                {
                    Position = AxisPosition.Left
                };

                List<Series> s1 = new List<Series>();

                return (xaxis, yaxis, s1);
            }
        }
    }
}
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
        private readonly ReplaySubject<Series> refreshSubject = new ReplaySubject<Series>();
        private readonly ReplaySubject<(string groupkey, string key, double value)> itemSubject = new ReplaySubject<(string groupkey, string key, double value)>();
        private readonly ReplaySubject<bool> stackSubject = new ReplaySubject<bool>();
        private readonly SynchronizationContext scheduler;
        private readonly IDisposable? disposable;
        private readonly object lck = new object();
        private readonly OxyPlotModel plotModel;
        private int columnCount = 0;

        public StackedBarModel(OxyPlotModel plotModel, SynchronizationContext? scheduler = null)
        {
            this.scheduler = scheduler ?? SynchronizationContext.Current;
            PlotBuilder.Build(plotModel);
            this.plotModel = plotModel;
            disposable ??= itemSubject.Buffer(TimeSpan.FromMilliseconds(300))
                .Where(a => a.Any())
                .CombineLatest(stackSubject.StartWith(true).DistinctUntilChanged(), (a, b) => (a, b))
                .Select(c => Combine(c.a, c.b))
                .SubscribeOn(scheduler)
                .Subscribe(series =>
                {
                    lock (plotModel)
                        plotModel.InvalidatePlot(true);
                });


            Unit Combine(IList<(string groupkey, string key, double value)> items, bool stack)
            {
                lock (plotModel)
                {
                    if (plotModel.DefaultXAxis == null)
                    {
                        throw new Exception($"plotModel needs a {plotModel.DefaultXAxis} (can't be null)");
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
                    if (plotModel.DefaultXAxis is CategoryAxis axis)
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

                ColumnSeries FindSeries(string key, IEnumerable<ColumnSeries> series, bool stack)
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
                        lock (plotModel)
                            plotModel.Series.Add(cSerie);
                    }

                    return cSerie;
                }
            }


            IEnumerable<ColumnSeries> EnumerateColumnSeries(bool stack)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    if (plotModel.Series[i] is ColumnSeries cSeries)
                    {
                        cSeries.IsStacked = stack;
                        yield return cSeries;
                    }
                }
            }
        }

        public void Reset()
        {
            scheduler.Post((a) =>
            {
                lock (plotModel)
                {
                    columnCount = 0;
                    while (plotModel.Series.Any())
                        plotModel.Series.Remove(plotModel.Series.First());

                    if (plotModel.DefaultXAxis is CategoryAxis categoryAxis)
                        categoryAxis.Labels.Clear();
                    plotModel.InvalidatePlot(false);
                }
            }, null);
        }

        public void OnCompleted()
        {
            // disposable.Dispose();
            //Reset();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException($"Error in {nameof(StackedBarModel)}");
        }

        public void OnNext(bool value)
        {
            stackSubject.OnNext(value);
        }

        public void OnNext((string groupkey, string key, double value) value)
        {
            itemSubject.OnNext(value);
        }
    }

    public static class PlotBuilder
    {
        public static void Build(OxyPlotModel plotModel)
        {
            (Axis xaxis, Axis yaxis, _) = BuildComponents();

            Construct(xaxis, yaxis);

    
            OxyPlotModel Construct(Axis xaxis, Axis yaxis)
            {
                xaxis.MajorGridlineStyle = LineStyle.Solid;
                xaxis.MinorGridlineStyle = LineStyle.Dot;
                yaxis.MajorGridlineStyle = LineStyle.Dot;
                xaxis.MinorGridlineStyle = LineStyle.Dot;

                plotModel.Axes.Add(xaxis);
                plotModel.Axes.Add(yaxis);
                //foreach (var series in s1)
                //    plotModel.Series.Add(series);

                return plotModel;
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
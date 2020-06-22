using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    public class StackedBarModel : IObserver<(string groupkey, string key, double value)>, IObserver<bool>
    {
        private readonly Lazy<PlotModel> model;
        private readonly ReplaySubject<Series.Series> refreshSubject = new ReplaySubject<Series.Series>();
        private readonly ReplaySubject<(string groupkey, string key, double value)> itemSubject = new ReplaySubject<(string groupkey, string key, double value)>();
        private readonly ReplaySubject<bool> stackSubject = new ReplaySubject<bool>();
        private readonly IScheduler scheduler;
        private IDisposable disposable;
        object lck = new object();
        public StackedBarModel(IScheduler? scheduler = null)
        {
            this.scheduler = scheduler ?? Scheduler.Default;
            model = new Lazy<PlotModel>(() => PlotBuilder.Build());
            Init();

        }

        private void Init()
        {
            itemSubject.Subscribe(a =>
            {

            },e=>
            {
            });

            disposable ??= itemSubject.CombineLatest(stackSubject.StartWith(true), (a, b) => (a, b))
                .Select(c => Combine(c.a, c.b))
                          .ObserveOn(scheduler)
                .Buffer(TimeSpan.FromMilliseconds(300))
                .SubscribeOn(scheduler)
                .Subscribe(series =>
                {
                    if (series.Any())
                    {
                        lock (model)
                            model.Value.InvalidatePlot(true);
                    }
                });
        }

        public PlotModel Model => model.Value;

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


        public Unit Combine((string groupkey, string key, double value) item, bool stack)
        {
            lock (model)
            {
                if (model.Value.DefaultXAxis == null)
                {
                    throw new Exception($"PlotModel needs a {model.Value.DefaultXAxis} (can't be null)");
                }
                Init();
                ColumnSeries s = FindSeries(item.groupkey, stack);
                var labels = (model.Value.DefaultXAxis as CategoryAxis).Labels;
                if (labels.Contains(item.key) == false)
                    labels.Add(item.key);

                s.Items.Add(new ColumnItem(item.value));

                return Unit.Default;
            }
        }

        int columnCount = 0;

        private ColumnSeries FindSeries(string key, bool stack)
        {
            ColumnSeries series = null;
            for (int i = 0; i < columnCount; i++)
            {
                if (model.Value.Series[i].Title == key)
                {
                    series = model.Value.Series[i] as ColumnSeries;
                    series.IsStacked = stack;
                }
            }
            if (series == null)
            {
                series = new ColumnSeries { Title = key, IsStacked = stack };
                columnCount++;
                lock (model)
                    model.Value.Series.Add(series);
            }

            return series;
        }

        public void Reset()
        {
            scheduler.Schedule(() =>
            {
                columnCount = 0;
                while (model.Value.Series.Any())
                    model.Value.Series.Remove(model.Value.Series.First());

                (model.Value.DefaultXAxis as CategoryAxis).Labels.Clear();
                model.Value.InvalidatePlot(false);
            });
        }


    }



    public static class PlotBuilder
    {
        public static PlotModel Build()
        {
            (Axis xaxis, Axis yaxis, List<Series.Series> s1) = BuildComponents();

            var model = Construct(xaxis, yaxis, s1);

            return model;


            static PlotModel Construct(Axis xaxis, Axis yaxis, List<Series.Series> s1)
            {

                xaxis.MajorGridlineStyle = LineStyle.Solid;
                xaxis.MinorGridlineStyle = LineStyle.Dot;
                yaxis.MajorGridlineStyle = LineStyle.Dot;
                xaxis.MinorGridlineStyle = LineStyle.Dot;

                var model = new PlotModel();

                model.Axes.Add(xaxis);
                model.Axes.Add(yaxis);
                //foreach (var series in s1)
                //    model.Series.Add(series);

                return model;
            }

            static (Axis, Axis, List<Series.Series>) BuildComponents()
            {
                CategoryAxis xaxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom
                };

                LinearAxis yaxis = new LinearAxis
                {
                    Position = AxisPosition.Left
                };

                List<Series.Series> s1 = new List<Series.Series>();

                return (xaxis, yaxis, s1);
            }
        }




    }
}

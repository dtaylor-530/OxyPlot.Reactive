using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace OxyPlot.Reactive
{
    public class StackedBarModel : IObserver<(string groupkey, string key, double value)>, IObserver<bool>
    {
        private readonly Lazy<PlotModel> model;
        private readonly Subject<Series.Series> refreshSubject = new Subject<Series.Series>();
        private readonly Subject<(string groupkey, string key, double value)> itemSubject = new Subject<(string groupkey, string key, double value)>();
        private readonly Subject<bool> stackSubject = new Subject<bool>();
        private readonly IDispatcher dispatcher;
        private IDisposable disposable;

        public StackedBarModel(IDispatcher dispatcher)
        {
            model = new Lazy<PlotModel>(() => PlotBuilder.Build());
            Init();
            this.dispatcher = dispatcher;
        }

        private void Init()
        {
            disposable ??= itemSubject.CombineLatest(stackSubject.StartWith(true), (a, b) => (a, b))
                .Select(c => Combine(c.a, c.b))
                .Buffer(TimeSpan.FromMilliseconds(300))
                .Subscribe(series =>
                {
                    if (series.Any())
                    {
                        dispatcher.Invoke(() =>
                        {
                            model.Value.InvalidatePlot(true);
                        });
                    }
                });
        }

        public PlotModel Model => model.Value;

        public void OnCompleted()
        {
            //throw new NotImplementedException();
            disposable.Dispose();
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
                model.Value.Series.Add(series);
            }

            return series;
        }

        public void Reset()
        {
            dispatcher.Invoke(() =>
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

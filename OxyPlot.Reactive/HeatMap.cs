#nullable enable
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using e = System.Linq.Enumerable;

namespace OxyPlot.Reactive
{
    public class HeatMap : IObserver<KeyValuePair<(string, string), double>>
    {
        public static readonly OxyColor foreground = OxyColors.SteelBlue;
        protected readonly ISubject<KeyValuePair<(string, string), double>> refreshSubject = new Subject<KeyValuePair<(string, string), double>>();
        protected readonly Lazy<PlotModel> plotModel;
        protected readonly object lck = new object();
        private readonly IComparer<string>? hNamesComparer;
        private readonly IComparer<string>? vNamesComparer;
        //private readonly string? hAxisKey;
        //private readonly string? vAxisKey;
        protected readonly IScheduler scheduler;
        readonly Dictionary<(string, string), double> dictionary = new Dictionary<(string, string), double>();

        public HeatMap(string? hAxisKey = null, string? vAxisKey = null, IScheduler? scheduler=null)
        {
            //this.hAxisKey = hAxisKey;
            //this.vAxisKey = vAxisKey;
            this.scheduler = scheduler?? Scheduler.Default;
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(100)).Where(e.Any).Subscribe(Refresh);
            plotModel = new Lazy<PlotModel>(() => CreateModel(hAxisKey, vAxisKey));
        }


        public HeatMap(
            IComparer<string>? hNamesComparer = null,
            IComparer<string>? vNamesComparer = null,
            string? hAxisKey = null,
            string? vAxisKey = null,
            IScheduler? scheduler =null) : this( hAxisKey, vAxisKey, scheduler:scheduler)
        {
            this.hNamesComparer = hNamesComparer ?? StringComparer.InvariantCultureIgnoreCase;
            this.vNamesComparer = vNamesComparer ?? StringComparer.InvariantCultureIgnoreCase;
        }


        private async void Refresh(IEnumerable<KeyValuePair<(string, string), double>> kvps)
        {
            var item = await Task.Run(() =>
             {
                 lock (lck)
                 {
                     foreach (var kvp in kvps)
                         dictionary[kvp.Key] = kvp.Value;

                     string[] hNames = dictionary.Select(a => a.Key.Item1).Distinct().OrderBy(a => a, hNamesComparer).ToArray();
                     string[] vNames = dictionary.Select(a => a.Key.Item2).Distinct().OrderBy(a => a, vNamesComparer).ToArray();
                     var values = dictionary.Select(a => a.Value).ToArray();
                     double min = values.Min();
                     double max = values.Max();

                     return (dictionary.ToMultiDimensionalArray(hNames, vNames), min, max, hNames, vNames);
                 }
             });

            var (data, min, max, hNames, vNames) = item;
            scheduler.Schedule(() =>
           {
               AlterModel(plotModel.Value, min, max, hNames, vNames);
               (plotModel.Value.Series[0] as HeatMapSeries).Data = data;
               plotModel.Value.InvalidatePlot(true);
           });
        }

        public PlotModel Model => plotModel.Value;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(KeyValuePair<(string, string), double> kvp)
        {

            refreshSubject.OnNext(kvp);
        }

        public void OnNext(KeyValuePair<string, AxisPosition> value)
        {
            throw new NotImplementedException();
        }

        private static PlotModel CreateModel(string? horizontalAxis, string? verticalAxis)
        {
            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground
            };

            plotModel.Axes.Clear();


            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = horizontalAxis,
                ItemsSource = new string[0],
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground,
                Title = horizontalAxis
            });

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = verticalAxis,
                ItemsSource = new string[0],
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground,
                Title = verticalAxis
            });

            plotModel.Axes.Add(new LinearColorAxis
            {
                // Pearson color scheme from blue over white to red.
                Palette = OxyPalettes.BlueWhiteRed31,
                Position = AxisPosition.Top,
                Minimum = -1,
                Maximum = 1,
                TicklineColor = OxyColors.Transparent
            });

            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = 0,
                Y0 = 0,
                Y1 = 0,
                XAxisKey = horizontalAxis,
                YAxisKey = verticalAxis,
                RenderMethod = HeatMapRenderMethod.Rectangles,
                LabelFontSize = 0.12,
                LabelFormatString = ".00"
            };

            plotModel.Series.Add(heatMapSeries);

            return plotModel;
        }

        private static void AlterModel(PlotModel plotModel, double min, double max, string[] horizontalNames, string[] verticalNames)
        {
            var hmSeries = plotModel.Series.First() as HeatMapSeries;
            for (int i = plotModel.Axes.Count - 1; i + 1 > 0; i--)
            {
                if (plotModel.Axes[i] is LinearColorAxis linearColorAxis)
                {
                    if (-min < max)
                    {
                        linearColorAxis.Maximum = max;
                        linearColorAxis.Minimum = min < 0 ? max > 0 ? -max : 0 : 0;

                    }
                    else
                    {
                        linearColorAxis.Minimum = min;
                        linearColorAxis.Maximum = max > 0 ? min < 0 ? -min : 0 : 0;
                    }

                    if (linearColorAxis.Minimum == 0)
                    {
                        linearColorAxis.Palette = OxyPalettes.Hot(10);
                    }
                    else if (linearColorAxis.Maximum == 0)
                    {
                        linearColorAxis.Palette = OxyPalettes.Cool(10);
                    }
                    else
                    {
                        linearColorAxis.Palette = OxyPalettes.BlueWhiteRed31;
                    }
                }
                else if (plotModel.Axes[i] is CategoryAxis categoryAxis)
                {
                    if (categoryAxis.Position == AxisPosition.Bottom)
                    {
                        if (horizontalNames.SequenceEqual(categoryAxis.ItemsSource.Cast<string>()) == false)
                        {
                            hmSeries.X1 = horizontalNames.Length - 1;
                            categoryAxis.ItemsSource = horizontalNames;
                        }
                    }
                    if (categoryAxis.Position == AxisPosition.Left)
                    {
                        if (verticalNames.SequenceEqual(categoryAxis.ItemsSource.Cast<string>()) == false)
                        {
                            categoryAxis.ItemsSource = verticalNames;
                            hmSeries.Y1 = verticalNames.Length - 1;
                        }
                    }
                }

            }
        }

    }

    public static class Helper
    {
        public static T[,] ToMultiDimensionalArray<T>(this Dictionary<(string, string), T> dictionary, string[] hNames, string[] vNames)
        {
            T[,] result = new T[hNames.Length, vNames.Length];

            for (int i = 0; i < hNames.Length; i++)
            {
                for (int k = 0; k < vNames.Length; k++)
                {
                    if (dictionary.ContainsKey((hNames[i], vNames[k])))
                    {
                        result[i, k] = dictionary[(hNames[i], vNames[k])];
                    }
                    else
                    {
                        result[i, k] = default;
                    }
                }
            }

            return result;
        }
    }
}
#nullable enable

using OxyPlot.Axes;
using OxyPlot.Reactive.Infrastructure;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using e = System.Linq.Enumerable;

namespace OxyPlot.Reactive
{
    public class HeatMap : IObserver<KeyValuePair<(string, string), double>>, IMixedScheduler
    {
        public static readonly OxyColor foreground = OxyColors.SteelBlue;
        protected readonly ISubject<KeyValuePair<(string, string), double>> refreshSubject = new Subject<KeyValuePair<(string, string), double>>();
        protected readonly PlotModel plotModel;
        private readonly IComparer<string>? hNamesComparer;
        private readonly IComparer<string>? vNamesComparer;
        private readonly Dictionary<(string, string), double> dictionary = new Dictionary<(string, string), double>();

        public HeatMap(PlotModel plotModel, string? hAxisKey = null, string? vAxisKey = null, IComparer<string>? hNamesComparer = null,
            IComparer<string>? vNamesComparer = null, IScheduler? scheduler = null, SynchronizationContext? synchronisationContext = null)
        {
            this.hNamesComparer = hNamesComparer ?? StringComparer.InvariantCultureIgnoreCase;
            this.vNamesComparer = vNamesComparer ?? StringComparer.InvariantCultureIgnoreCase;

            if (scheduler == null)
                this.Context = synchronisationContext ?? SynchronizationContext.Current;
            else
                this.Scheduler = scheduler;

            lock (plotModel)
                this.plotModel = CreateModel(plotModel, hAxisKey, vAxisKey);

            refreshSubject.Buffer(TimeSpan.FromMilliseconds(300)).Where(e.Any).Subscribe(Refresh);
        }

        private async void Refresh(IEnumerable<KeyValuePair<(string, string), double>> kvps)
        {
            var (data, min, max, hNames, vNames) = await Task.Run(() =>
            {
                lock (dictionary)
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

            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (plotModel)
                {
                    AlterModel(plotModel, min, max, hNames, vNames);

                    if (plotModel.Series[0] is HeatMapSeries heatMapSeries)
                    {
                        heatMapSeries.Data = data;
                        plotModel.InvalidatePlot(true);
                    }
                }
            });
        }

        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

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

        private static PlotModel CreateModel(PlotModel plotModel, string? horizontalAxis, string? verticalAxis)
        {
            const string horizontal = "horizontal";
            const string vertical = "vertical";

            plotModel.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            plotModel.PlotAreaBorderColor = foreground;
            plotModel.TextColor = foreground;
            plotModel.TitleColor = foreground;
            plotModel.SubtitleColor = foreground;

            plotModel.Axes.Clear();

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = horizontalAxis ?? horizontal,
                ItemsSource = new string[0],
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground,
                Title = horizontalAxis ?? horizontal
            });

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = verticalAxis ?? vertical,
                ItemsSource = new string[0],
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground,
                Title = verticalAxis ?? vertical,
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
                X1 = 100,
                Y0 = 0,
                Y1 = 100,
                // Will cause null reference exception if Data not initialised
                Data = new double[0, 0],
                XAxisKey = horizontalAxis ?? horizontal,
                YAxisKey = verticalAxis ?? vertical,
                RenderMethod = HeatMapRenderMethod.Rectangles,
                LabelFontSize = 0.12,
                LabelFormatString = ".00"
            };

            plotModel.Series.Add(heatMapSeries);

            return plotModel;
        }

        private static void AlterModel(PlotModel plotModel, double min, double max, string[] horizontalNames, string[] verticalNames)
        {
            if (plotModel.Series.First() is HeatMapSeries hmSeries)
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
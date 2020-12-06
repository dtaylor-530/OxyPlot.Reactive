using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using splot = ScottPlot;

namespace ReactivePlot.ScottPlot
{
    public class OHLCModel
    {
        private List<OHLC> data = new List<OHLC>();
        int index = 0;
        private readonly WpfPlot wpfPlot;

        public OHLCModel(WpfPlot wpfPlot, IObservable<OHLC> dataObservable, IObservable<Unit> renderObservable = null, IScheduler uiScheduler = null)
        {
            uiScheduler ??= Scheduler.CurrentThread;

            this.wpfPlot = wpfPlot;

            dataObservable.Subscribe(d =>
            {
                data.Add(d);
            });

            (renderObservable ?? dataObservable.Select(a => Unit.Default))
                .SubscribeOn(uiScheduler)
                .Subscribe(a =>
            {
                Render();
                //mainThread.Send(a => Render(), null);
            });

        }


        void Render()
        {
            //if (AutoAxisCheckbox.IsChecked == true)
            // wpfPlot.plt.AxisAuto();
            var arr = data.ToArray();
            var signal = wpfPlot.plt.PlotCandlestick(arr);
            if (arr.Length >= 100)
            {
                Task.Run(() => splot.Statistics.Finance.Bollinger(arr))
                    .ContinueWith(async a =>
                {
                    var (sma, bolL, bolU) = await a;
                    double[] xs = DataGen.Consecutive(arr.Length);
                    wpfPlot.plt.PlotScatter(xs, bolL, color: Color.Blue, markerSize: 0);
                    wpfPlot.plt.PlotScatter(xs, bolU, color: Color.Blue, markerSize: 0);
                    wpfPlot.plt.PlotScatter(xs, sma, color: Color.Blue, markerSize: 0, lineStyle: LineStyle.Dash);
                }, TaskScheduler.FromCurrentSynchronizationContext());

            }

            double[] autoAxisLimits = wpfPlot.plt.AxisAuto(verticalMargin: .5);
            double oldX2 = autoAxisLimits[1];
            wpfPlot.plt.Axis(x2: oldX2 + 10);

            wpfPlot.Render(skipIfCurrentlyRendering: true);
        }



        //private void DisableAutoAxis(object sender, RoutedEventArgs e)
        //{
        //    double[] autoAxisLimits = wpfPlot1.plt.AxisAuto(verticalMargin: .5);
        //    double oldX2 = autoAxisLimits[1];
        //    wpfPlot1.plt.Axis(x2: oldX2 + 1000);
        //}
    }


}

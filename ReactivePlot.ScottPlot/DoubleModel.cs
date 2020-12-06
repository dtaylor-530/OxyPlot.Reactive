using ReactivePlot.Model;
using ScottPlot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ReactivePlot.ScottPlot
{
    /// <summary>
    /// Interaction logic for LiveDataGrowing.xaml
    /// </summary>
    public class DoubleModel : ISinglePlotModel<double>
    {
        private double[] data = new double[100_000];
        int index0 = 0;
        private readonly WpfPlot wpfPlot;
        private readonly PlottableSignal signal;

        public DoubleModel(WpfPlot wpfPlot)
        {
            this.wpfPlot = wpfPlot;
            signal = wpfPlot.plt.PlotSignal(data);
            signal.maxRenderIndex = 1;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Invalidate(bool v)
        {
            double[] autoAxisLimits = wpfPlot.plt.AxisAuto(verticalMargin: .5);
            double oldX2 = autoAxisLimits[1];
            wpfPlot.plt.Axis(x2: oldX2 + 100);
            wpfPlot.Reset();
            wpfPlot.Render(skipIfCurrentlyRendering: v);
        }

        public void Add(IEnumerable items)
        {
            if (items is IReadOnlyCollection<double> { } dItems2)
                Add(dItems2);
            else
                throw new NotImplementedException();
        }

        public void Add(IReadOnlyCollection<double> items)
        {
            foreach (var item in items)
            {
                signal.maxRenderIndex = index0;
                data[index0++] = (double)item;
            }
        }
    }

    /// <summary>
    /// Interaction logic for LiveDataGrowing.xaml
    /// </summary>
    public partial class DoubleModel<TR> : ISinglePlotModel<TR>
        where TR : IIndex, IValue<double>

    {
        private double[] data = new double[100_000];
        int index0 = 0;
        private readonly WpfPlot wpfPlot;
        private readonly PlottableSignal signal;

        public DoubleModel(WpfPlot wpfPlot)
        {
            this.wpfPlot = wpfPlot;
            signal = wpfPlot.plt.PlotSignal(data);
            signal.maxRenderIndex = 1;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Invalidate(bool v)
        {
            double[] autoAxisLimits = wpfPlot.plt.AxisAuto(verticalMargin: .5);
            double oldX2 = autoAxisLimits[1];
            wpfPlot.plt.Axis(x2: oldX2 + 100);

            wpfPlot.Render(skipIfCurrentlyRendering: v);
        }

        public void Add(IEnumerable items)
        {
            throw new NotImplementedException();
            //foreach (var item in items)
            //{
            //    signal.maxRenderIndex = index0;
            //    if(item is Iind)
            //    data[index0++] = (double)item;
            //}
        }



        public void Add(IReadOnlyCollection<TR> items)
        {
            foreach (var item in items)
            {
                signal.maxRenderIndex = index0;
                data[item.Index] = item.Value;
            }
        }

        //private void DisableAutoAxis(object sender, RoutedEventArgs e)
        //{
        //    double[] autoAxisLimits = wpfPlot1.plt.AxisAuto(verticalMargin: .5);
        //    double oldX2 = autoAxisLimits[1];
        //    wpfPlot1.plt.Axis(x2: oldX2 + 1000);
        //}
    }
}

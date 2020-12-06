using ReactivePlot.Model;
using ScottPlot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using splot = ScottPlot;

namespace ReactivePlot.ScottPlot
{
    public class StatisticsModel: ISinglePlotModel<(double x, double y)>
    {
        private readonly WpfPlot wpfPlot;
        private readonly List<double> listX = new List<double>(), listY = new List<double>();

        public StatisticsModel(WpfPlot wpfPlot)
        {
            this.wpfPlot = wpfPlot;
      
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Invalidate(bool v)
        { 
            // create a Population object from the data
            var pop = new splot.Statistics.Population(listX.ToArray());

            // display the original values scattered vertically

            wpfPlot.plt.PlotScatter(pop.values, listY.ToArray(), markerSize: 10,
                markerShape: MarkerShape.openCircle, lineWidth: 0);

            // display the bell curve for this distribution
            double[] curveXs = DataGen.Range(pop.minus2stDev, pop.plus2stDev, 0.1);
            double[] curveYs = pop.GetDistribution(curveXs, false);
            wpfPlot. plt.PlotScatter(curveXs, curveYs, markerSize: 0, lineWidth: 2);

            wpfPlot.Render(skipIfCurrentlyRendering: v);
        }

        public void Add(IEnumerable items)
        {
            if (items is IReadOnlyCollection<double> { } dItems2)
                Add(dItems2);
            else
                throw new NotImplementedException();
            //foreach (var item in items)
            //{
            //    signal.maxRenderIndex = index0;
            //    if(item is Iind)
            //    data[index0++] = (double)item;
            //}
        }



        public void Add(IReadOnlyCollection<(double x, double y)> items)
        {
            foreach (var (x,y) in items)
            {
                listX.Add(x);
                listY.Add(y);
            }
        }
    }
}

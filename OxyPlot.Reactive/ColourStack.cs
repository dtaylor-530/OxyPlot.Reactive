using System;
using System.Collections.Generic;
using System.Text;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Reactive;

namespace OxyPlotEx.ViewModel
{
    //class ColourStack : SinglePlotModel<string>
    //{
    //    public ColourStack(IDispatcher dispatcher, PlotModel plotModel) : base(dispatcher, plotModel)
    //    {
    //    }

  

   
    //    //protected override void Refresh(IList<Unit> units)
    //    //{
    //    //    var items = new List<RectangleWithValue>();
    //    //    for (int i = 0; i < NumberOfItems; i++)
    //    //    {
    //    //        this.Items.Add(new RectangleWithValue(i));
    //    //    }

    //    //    this.PlotModel = new PlotModel();

    //    //    this.PlotModel.Axes.Add(new LinearColorAxis
    //    //    {
    //    //        Position = AxisPosition.Right,
    //    //        Palette = OxyPalettes.Jet(100)
    //    //    });

    //    //    this.PlotModel.Series.Add(new RectangleSeries
    //    //    {
    //    //        ItemsSource = this.Items,
    //    //        Mapping = obj =>
    //    //        {
    //    //            var rectangleWithValue = (RectangleWithValue)obj;

    //    //            return new RectangleItem(
    //    //                rectangleWithValue.X1,
    //    //                rectangleWithValue.X2,
    //    //                rectangleWithValue.Y1,
    //    //                rectangleWithValue.Y2,
    //    //                rectangleWithValue.Value);
    //    //        }
    //    //    });
    //    //}



    //}

    public class RectangleWithValue
    {
        public double X1;

        public double X2;

        public double Y1;

        public double Y2;

        public double Value;

        public RectangleWithValue(int seed)
        {
            this.X1 = seed;
            this.X2 = 2 * seed;
            this.Y1 = seed;
            this.Y2 = 2 * seed;
            this.Value = seed;
        }
    }




}

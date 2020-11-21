
using OxyPlot;
using OxyPlot.Axes;
using System;

namespace ReactivePlot.OxyPlot.Common
{
    public struct DataPointProvider : IDataPointProvider
    {

        private readonly bool isDateTime;
        private readonly double? x;
        private readonly DateTime? x1;
        private readonly double y;

        public DataPointProvider(DateTime x, double y)
        {
            x1 = x;
            this.x = null;
            this.y = y;
            isDateTime = true;
        }

        public DataPointProvider(double x, double y)
        {
            this.x = x;
            this.y = y;
            this.x1 = null;
            isDateTime = false;
        }

        public DataPoint GetDataPoint()
        {
            return isDateTime ?
                new DataPoint(DateTimeAxis.ToDouble(x1.Value), y) :
                new DataPoint(x.Value, y);
        }
    }
}

using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyPlotEx
{
    public struct DateTimePoint: OxyPlot.IDataPointProvider
    {

        public DateTimePoint(DateTime dateTime, double value)
        {
            DateTime = dateTime;
            Value = value;
        }

        public DateTime DateTime { get; }

        public double Value { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTime.Ticks, Value);
        }

        public override string ToString()
        {
            return $"{DateTime.ToString("F")}, {Value}";
        }
    }
}

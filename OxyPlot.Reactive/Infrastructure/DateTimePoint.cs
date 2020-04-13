using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyPlotEx
{
    public interface IDateTimePoint : IDataPointProvider
    {
        DateTime DateTime { get; }

        double Value { get; }


    }

    public interface IKey<T>
    {
        T Key { get; }
    }

    public interface IDateTimeKeyPoint<T> : IDateTimePoint, IKey<T>
    {

    }

    public interface IDataPointKeyProvider<T>: IDataPointProvider,IKey<T>
    {
     
    }

    public struct DateTimePoint : IDateTimeKeyPoint<string>
    {

        public DateTimePoint(DateTime dateTime, double value, string key)
        {
            DateTime = dateTime;
            Value = value;
            Key = key;

        }

        public DateTimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
        {
        }

        public string Key { get; }

        public DateTime DateTime { get; }

        public double Value { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(DateTime), Value);
        }

        public override string ToString()
        {
            return $"{DateTime.ToString("F")}, {Value}";
        }
    }

    public struct DateTimePoint<T> : IDateTimeKeyPoint<T>
    {

        public DateTimePoint(DateTime dateTime, double value, T key)
        {
            DateTime = dateTime;
            Value = value;
            Key = key;

        }

        public DateTimePoint(DateTime dateTime, double value) : this(dateTime, value, default)
        {
        }

        public T Key { get; }

        public DateTime DateTime { get; }

        public double Value { get; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(DateTime), Value);
        }

        public override string ToString()
        {
            return $"{DateTime.ToString("F")}, {Value}";
        }
    }
}

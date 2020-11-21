using System;

namespace ReactivePlot.Model
{
    public struct TimeErrorPoint
    {
        public TimeErrorPoint(DateTime dateTime, double value, double deviation)
        {
            DateTime = dateTime;
            Value = value;
            Deviation = deviation;
        }

        public DateTime DateTime { get; }

        public double Value { get; }

        public double Deviation { get; }

        public double Lower => Value - Deviation;

        public double Upper => Value + Deviation;
    }
}
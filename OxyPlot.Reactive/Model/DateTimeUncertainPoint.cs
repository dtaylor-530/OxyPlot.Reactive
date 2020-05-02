using System;

namespace OxyPlot.Reactive.Model
{

    public struct DateTimeUncertainPoint
    {
        public DateTimeUncertainPoint(DateTime dateTime, double value, double deviation)
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

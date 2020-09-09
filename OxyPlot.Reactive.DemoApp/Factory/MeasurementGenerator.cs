using System;

namespace OxyPlotEx.DemoApp
{
    internal static class MeasurementGenerator
    {
        public static Tuple<DateTime, double> Generate(int iteration, Func<int, double> equation) =>
            Tuple.Create(new DateTime() + new TimeSpan(iteration * 100000000000000), equation(iteration));
    }
}
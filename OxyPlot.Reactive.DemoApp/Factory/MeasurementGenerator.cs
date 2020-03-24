using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyPlotEx.DemoApp
{


    internal static class MeasurementGenerator
    {
        public static Tuple<DateTime, double> Generate(int iteration, Func<int, double> equation) =>
            Tuple.Create(new DateTime() + new TimeSpan(iteration * 100000000000000), equation(iteration));

    }
}

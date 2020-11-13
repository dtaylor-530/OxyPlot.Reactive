using System;
using System.Collections.Generic;
using System.Text;

namespace OxyPlot.Reactive.Common
{
    public class GroupKeyFactory
    {
        public static string Create(double val, double power)
        {
            int v = (int)Math.Log(val, power);

            var min = Math.Pow(power, v);
            var max = Math.Pow(power, v + 1);
            return $"{min:N} - {max:N}";
        }
    }
}

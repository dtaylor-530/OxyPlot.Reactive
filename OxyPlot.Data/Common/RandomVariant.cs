using System;
using System.Collections.Generic;

namespace ReactivePlot.Data.Common
{
    public static class RandomVariant
    {
        /// <summary>
        /// Generate random variants of a signal
        /// </summary>
        /// <param name="val"></param>
        /// <param name="signals"></param>
        /// <param name="r"></param>
        /// <param name="iteration"></param>
        /// <param name="isObserved"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, Tuple<DateTime, double>>> Generate(double val, Dictionary<string, Tuple<double, int>> signals, Random r, int iteration, bool isObserved = true)
        {
            foreach (var signal in signals)
            {
                double rv = MathsHelper.Generate(val, signal.Value.Item1, r, signal.Value.Item2);
                yield return new KeyValuePair<string, Tuple<DateTime, double>>(signal.Key, Tuple.Create(new DateTime().AddDays(iteration * 1000), rv));
            }
        }

        /// <summary>
        /// Generate random variants of a signal (includes the lower upper bounds of the deviation along with the variant)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="signals"></param>
        /// <param name="r"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, Tuple<double, double, double>>> GenerateWithDeviation(double val, Dictionary<string, Tuple<double, int>> signals, Random r, int iteration)
        {
            foreach (var signal in signals)
            {
                double rv = MathsHelper.Generate(val, signal.Value.Item1, r, signal.Value.Item2);

                // factor multiplied by the deviation
                var deviation = signal.Value.Item1 * signal.Value.Item2;
                yield return new KeyValuePair<string, Tuple<double, double, double>>(signal.Key, Tuple.Create(rv, rv - deviation, rv + deviation));
            }
        }
    }
}
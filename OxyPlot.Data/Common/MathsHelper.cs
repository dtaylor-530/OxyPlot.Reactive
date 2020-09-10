﻿using System;

namespace OxyPlot.Data.Common
{
    internal static class MathsHelper
    {
        public static double Generate(double mean, double stddeviation, Random rand, int factor = 1)
        {
            rand = rand ?? new Random();
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                       Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + stddeviation * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal * factor;
        }
    }
}
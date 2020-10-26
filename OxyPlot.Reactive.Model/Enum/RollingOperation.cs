using System;
using System.Collections.Generic;
using System.Text;

namespace OxyPlot.Reactive.Model.Enum
{
    public enum RollingOperation
    {
        Average,
        GeometricAverage,
        SquareMean,
        PopulationStandardDeviation,
        SampleStandardDeviation,
        PopulationVariance,
        SampleVariance,
        Max,
        Min,
        N,
        Range,
        Sum,
        MidRange,
        Difference,
        StandardError
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ReactivePlot.Model
{
    public struct DateTimeVar : IVar<DateTime>
    {
        public DateTime Var { get; }
    }   
    
    public struct DoubleVar : IVar<double>
    {
        public double Var { get; }
    }
}

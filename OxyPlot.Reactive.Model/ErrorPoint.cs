namespace ReactivePlot.Model
{
    public struct ErrorPoint
    {
        public ErrorPoint(double value, double error)
        {
            Value = value;
            Deviation = error;
        }

        public double Value { get; }

        public double Deviation { get; }
    }
}

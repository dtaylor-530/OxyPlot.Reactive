namespace ReactivePlot.Model
{
    public struct DataUncertainPoint<T>
    {
        public DataUncertainPoint(T dateTime, double value, double deviation)
        {
            X = dateTime;
            Y = value;
            Deviation = deviation;
        }

        public T X { get; }

        public double Y { get; }

        public double Deviation { get; }

        public double Lower => Y - Deviation;

        public double Upper => Y + Deviation;

        public override string ToString()
        {
            return $"{X.ToString()}, {Y.ToString("n")}";
        }
    }
}
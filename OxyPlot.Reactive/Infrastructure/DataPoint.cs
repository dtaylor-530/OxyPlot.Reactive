namespace OxyPlotEx
{
    public struct DataPoint<T>
    {
        public DataPoint(T dateTime, double value)
        {
            X = dateTime;
            Y = value;
        }

        public T X { get; }

        public double Y { get; }

        public override string ToString()
        {
            return $"{X.ToString()}, {Y}";
        }
    }
}

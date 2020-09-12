namespace OxyPlot.Reactive.Model
{
    //public interface IPoint : IDataPointProvider
    //{
    //    double X { get; }

    //    double Y { get; }
    //}

    public struct DoublePoint<TKey> : IDoublePoint<TKey>
    {
        public DoublePoint(double var, double value, TKey key) : this()
        {
            Var = var;
            Value = value;
            Key = key;
        }

        public double Var { get; set; }

        public double Value { get; set; }

        public TKey Key { get; set; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(Var, Value);
        }
    }

    //public struct KeyPoint<TKey> : IDataPointProvider, IPoint<TKey>
    //{
    //    public KeyPoint(double constant, double variable, TKey key)
    //    {
    //        X = constant;
    //        Y = variable;
    //        Key = key;
    //    }

    //    public double X { get; }

    //    public double Y { get; }

    //    public TKey Key { get; }

    //    public override string ToString()
    //    {
    //        return $"{Key}, {X}, {Y}";
    //    }

    //    public DataPoint GetDataPoint()
    //    {
    //        return new DataPoint(X, Y);
    //    }
    //}
}
namespace ReactivePlot.Model
{

    public struct XY<T>
    {
        public XY(T constant, double variable)
        {
            X = constant;
            Y = variable;
        }

        public T X { get; }

        public double Y { get; }

        public override string ToString()
        {
            return $"{X}, {Y}";
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
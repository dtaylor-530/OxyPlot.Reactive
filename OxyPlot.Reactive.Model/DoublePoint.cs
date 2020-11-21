namespace ReactivePlot.Model
{
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

        public void Deconstruct(out double var, out double value)
        {
            var = Var;
            value = Value;
        }
    }
}
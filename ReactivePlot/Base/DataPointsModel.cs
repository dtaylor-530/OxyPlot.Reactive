#nullable enable

using System.Collections.Generic;

namespace ReactivePlot.Base
{
    public class DataPointsModel<TKey, TValue>
    {
        protected readonly Dictionary<TKey, ICollection<TValue>> DataPoints;
        protected readonly IEqualityComparer<TKey>? equalityComparer;

        public DataPointsModel(IEqualityComparer<TKey>? comparer = null)
        {
            DataPoints = GetDataPoints();
            this.equalityComparer = comparer;
        }

        protected virtual void AddToDataPoints(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (DataPoints)
            {
                foreach (var item in items)
                {
                    var newdp = item.Value;
                    if (!DataPoints.ContainsKey(item.Key))

                        DataPoints[item.Key] = CreateCollection();
                    DataPoints[item.Key].Add(newdp);
                }
            }
        }

        protected virtual ICollection<TValue> CreateCollection() => new List<TValue>();



        protected virtual void RemoveFromDataPoints(IEnumerable<TKey> keys)
        {
            lock (DataPoints)
            {
                foreach (var key in keys)
                    if (DataPoints.ContainsKey(key))
                        DataPoints.Remove(key);
            }
        }

        protected virtual Dictionary<TKey, ICollection<TValue>> GetDataPoints()
        {
            return equalityComparer == default ?
                   new Dictionary<TKey, ICollection<TValue>>() :
                new Dictionary<TKey, ICollection<TValue>>(equalityComparer);
        }
    }
}
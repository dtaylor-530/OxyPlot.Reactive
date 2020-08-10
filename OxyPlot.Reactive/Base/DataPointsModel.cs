#nullable enable
using System.Collections.Generic;

namespace OxyPlot.Reactive
{
    public class DataPointsModel<TKey, TValue>
    {
        protected readonly Dictionary<TKey, ICollection<TValue>> DataPoints;
        protected readonly IEqualityComparer<TKey>? comparer;

        public DataPointsModel(IEqualityComparer<TKey>? comparer = null)
        {
            DataPoints = GetDataPoints();
            this.comparer = comparer;
        }

        protected virtual void AddToDataPoints(KeyValuePair<TKey, TValue> item)
        {
            var newdp = item.Value;
            lock (DataPoints)
            {
                if (!DataPoints.ContainsKey(item.Key))
                    DataPoints[item.Key] = new List<TValue>();
                DataPoints[item.Key].Add(newdp);
            }
        }

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
            return comparer == default ?
                  new Dictionary<TKey, ICollection<TValue>>() :
                new Dictionary<TKey, ICollection<TValue>>(comparer);
        }
    }
}




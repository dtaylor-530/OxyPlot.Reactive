#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactivePlot.Base
{
    public class DataPointsModel<TKey, TValue>: IObserver<ISet<TKey>>
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

        protected virtual void Remove(ISet<TKey> keys)
        {
            lock (DataPoints)
            {
                foreach (var key in keys)
                    if (DataPoints.ContainsKey(key))
                        DataPoints.Remove(key);
            }
        }

        protected virtual void Reset()
        {
            lock (DataPoints)
                DataPoints.Clear();
        }


        protected virtual Dictionary<TKey, ICollection<TValue>> GetDataPoints()
        {
            return equalityComparer == default ?
                   new Dictionary<TKey, ICollection<TValue>>() :
                new Dictionary<TKey, ICollection<TValue>>(equalityComparer);
        }

        public virtual void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public virtual void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ISet<TKey> value)
        {
            if(value.Any())
            {
                this.Remove(value);
            }
            else
            {
                this.Reset();
            }
        }
    }
}
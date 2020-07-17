using Itenso.TimePeriod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Infrastructure
{
    public static class CollectionHelper
    {
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> enumerable, ITimeRange dateTimeRange, Func<T, DateTime> predicate)
        {
            return enumerable.Where(a =>
            {
                var pr = predicate.Invoke(a);
                return dateTimeRange.Start <= pr && dateTimeRange.End >= pr;
            });
        }

        public static IEnumerable<T> Filter<T>(this ICollection<T> enumerable, TimeSpan timeSpan, Func<T, DateTime> predicate)
        {
            DateTime lastDate = enumerable.Max(a => predicate(a));
            var dateTimeRange = new TimeRange(lastDate - timeSpan, lastDate);
            return enumerable.Where(a =>
            {
                var pr = predicate.Invoke(a);
                return dateTimeRange.Start <= pr && dateTimeRange.End >= pr;
            });
        }

        public static IEnumerable<IGrouping<ITimeRange, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, TimeSpan timeSpan, Func<T, DateTime> predicate)
        {
            Grouping<T> grouping = null;
            foreach (var (a, dt) in from b in enumerable select (b, predicate.Invoke(b)))
            {
                if (grouping == null || dt > grouping.Key.End)
                    yield return grouping = new Grouping<T>(new TimeRange(dt, dt + timeSpan), a);
                else
                    grouping.Add(a);
            }
        }


        public static IEnumerable<IGrouping<ITimeRange, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, IEnumerable<ITimeRange> ranges, Func<T, DateTime> predicate)
        {
            return from r in ranges
                   join prod in enumerable on true equals true
                   into temp
                   select new Grouping<T>(r, temp.Where(t => predicate.Invoke(t) >= r.Start && predicate.Invoke(t) <= r.End).ToArray());
        }


        class Grouping<T> : IGrouping<ITimeRange, T>
        {

            readonly List<T> elements = new List<T>();

            public ITimeRange Key { get; }

            public Grouping(ITimeRange key) => Key = key;

            public Grouping(ITimeRange key, params T[] elements) : this(key) { foreach (var elem in elements) Add(elem); }

            public void Add(T element) => elements.Add(element);

            public IEnumerator<T> GetEnumerator() => this.elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}

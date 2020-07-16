using Exceptionless.DateTimeExtensions;
using MoreLinq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OxyPlot.Reactive.Infrastructure
{
    public static class CollectionHelper
    {
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> enumerable, DateTimeRange dateTimeRange, Func<T, DateTime> predicate)
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
            var dateTimeRange = new DateTimeRange(lastDate - timeSpan, lastDate);
            return enumerable.Where(a =>
            {
                var pr = predicate.Invoke(a);
                return dateTimeRange.Start <= pr && dateTimeRange.End >= pr;
            });
        }

        public static IEnumerable<IGrouping<DateTimeRange, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, TimeSpan timeSpan, Func<T, DateTime> predicate)
        {
            Grouping<T> grouping = null;
            foreach (var (a, dt) in from b in enumerable select (b, predicate.Invoke(b)))
            {
                if (grouping == null || dt > grouping.Key.End)
                    yield return grouping = new Grouping<T>(new DateTimeRange(dt, dt + timeSpan), a);
                else
                    grouping.Add(a);
            }
        }


        public static IEnumerable<IGrouping<DateTimeRange, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, IEnumerable<DateTimeRange> ranges, Func<T, DateTime> predicate)
        {
            return from r in ranges
                   join prod in enumerable on true equals true
                   into temp
                   select new Grouping<T>(r, temp.Where(t => predicate.Invoke(t) >= r.Start && predicate.Invoke(t) <= r.End).ToArray());
        }


        class Grouping<T> : IGrouping<DateTimeRange, T>
        {

            readonly List<T> elements = new List<T>();

            public DateTimeRange Key { get; }

            public Grouping(DateTimeRange key) => Key = key;

            public Grouping(DateTimeRange key, params T[] elements) : this(key) { foreach (var elem in elements) Add(elem); }

            public void Add(T element) => elements.Add(element);

            public IEnumerator<T> GetEnumerator() => this.elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}

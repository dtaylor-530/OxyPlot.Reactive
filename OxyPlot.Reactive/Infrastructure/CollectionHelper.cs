using Exceptionless.DateTimeExtensions;
using MoreLinq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static IEnumerable<IGrouping<DateRange, T>> GroupBy<T>(this IOrderedEnumerable<T> enumerable, TimeSpan timeSpan, Func<T, DateTime> predicate)
        {
            Grouping<T> grouping = null;
            foreach (var (a, dt) in from b in enumerable select (b, predicate.Invoke(b)))
            {
                if (grouping == null || dt > grouping.Key.End)
                    yield return grouping = new Grouping<T>(new DateRange(dt, dt + timeSpan), a);
                else
                    grouping.Add(a);
            }
        }

        class Grouping<T> : IGrouping<DateRange, T>
        {

            readonly List<T> elements = new List<T>();

            public DateRange Key { get; }

            public Grouping(DateRange key) => Key = key;

            public Grouping(DateRange key, T element) : this(key) => Add(element);

            public void Add(T element) => elements.Add(element);

            public IEnumerator<T> GetEnumerator() => this.elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }



        public class DateRange
        {

            public DateRange(DateTime start, DateTime end)
            {
                this.Start = start;
                this.End = end;
            }

            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }
    }
}

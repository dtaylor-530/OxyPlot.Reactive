using Itenso.TimePeriod;
using LinqStatistics;
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

        //public static IEnumerable<IGrouping<ITimeRange, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, TimeSpan timeSpan, Func<T, DateTime> predicate)
        //{
        //    TimeGrouping1<T> grouping = null;
        //    foreach (var (a, dt) in from b in enumerable select (b, predicate.Invoke(b)))
        //    {
        //        if (grouping == null || dt > grouping.Key.End)
        //            yield return grouping = new TimeGrouping1<T>(new TimeRange(dt, dt + timeSpan), a);
        //        else
        //            grouping.Add(a);
        //    }
        //}

        public static IEnumerable<IGrouping<Range<DateTime>, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, TimeSpan timeSpan, Func<T, DateTime> predicate)
        {
            TimeGrouping<T> grouping = null;
            foreach (var (a, dt) in from b in enumerable select (b, predicate.Invoke(b)))
            {
                if (grouping == null || dt > grouping.Key.Max)
                    yield return grouping = new TimeGrouping<T>(new Range<DateTime>(dt, dt + timeSpan), a);
                else
                    grouping.Add(a);
            }
        }

        public static IEnumerable<IGrouping<Range<double>, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, Range<double> range, Func<T, double> predicate)
        {
            DoubleGrouping<T> grouping = null;
            foreach (var (a, dt) in from b in enumerable select (b, predicate.Invoke(b)))
            {
                if (grouping == null || dt > grouping.Key.Max)
                    yield return grouping = new DoubleGrouping<T>(new Range<double>(dt, dt + range.Max - range.Min), a);
                else
                    grouping.Add(a);
            }
        }

        //public static IEnumerable<IGrouping<ITimeRange, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, IEnumerable<ITimeRange> ranges, Func<T, DateTime> predicate)
        //{
        //    return from r in ranges
        //           join prod in enumerable on true equals true
        //           into temp
        //           select new TimeGrouping1<T>(r, temp.Where(t => predicate.Invoke(t) >= r.Start && predicate.Invoke(t) <= r.End).ToArray());
        //}

        public static IEnumerable<IGrouping<Range<DateTime>, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, IEnumerable<Range<DateTime>> ranges, Func<T, DateTime> predicate)
        {
            return from r in ranges
                   join prod in enumerable on true equals true
                   into temp
                   select new TimeGrouping<T>(r, temp.Where(t => predicate.Invoke(t) >= r.Min && predicate.Invoke(t) <= r.Max).ToArray());
        }

        public static IEnumerable<IGrouping<Range<double>, T>> GroupOn<T>(this IOrderedEnumerable<T> enumerable, IEnumerable<Range<double>> ranges, Func<T, double> predicate)
        {
            return from r in ranges
                   join prod in enumerable on true equals true
                   into temp
                   select new DoubleGrouping<T>(r, temp.Where(t => predicate.Invoke(t) >= r.Min && predicate.Invoke(t) <= r.Max).ToArray());
        }
    }

    internal class TimeGrouping1<T> : IGrouping<ITimeRange, T>
    {
        private readonly List<T> elements = new List<T>();

        public ITimeRange Key { get; }

        public TimeGrouping1(ITimeRange key) => Key = key;

        public TimeGrouping1(ITimeRange key, params T[] elements) : this(key)
        {
            foreach (var elem in elements) Add(elem);
        }

        public void Add(T element) => elements.Add(element);

        public IEnumerator<T> GetEnumerator() => this.elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class TimeGrouping<T> : IGrouping<Range<DateTime>, T>
    {
        private readonly List<T> elements = new List<T>();

        public Range<DateTime> Key { get; }

        public TimeGrouping(Range<DateTime> key) => Key = key;

        public TimeGrouping(Range<DateTime> key, params T[] elements) : this(key)
        {
            foreach (var elem in elements) Add(elem);
        }

        public void Add(T element) => elements.Add(element);

        public IEnumerator<T> GetEnumerator() => this.elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class DoubleGrouping<T> : IGrouping<Range<double>, T>
    {
        private readonly List<T> elements = new List<T>();

        public Range<double> Key { get; }

        public DoubleGrouping(Range<double> key) => Key = key;

        public DoubleGrouping(Range<double> key, params T[] elements) : this(key)
        {
            foreach (var elem in elements) Add(elem);
        }

        public void Add(T element) => elements.Add(element);

        public IEnumerator<T> GetEnumerator() => this.elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
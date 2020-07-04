using Exceptionless.DateTimeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxyPlot.Reactive.Infrastructure
{
    public static class CollectionHelper
    {
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> enumerable, DateTimeRange dateTimeRange, Func<T,DateTime> predicate)
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
    }
}

using System;
using System.Collections;

namespace ReactivePlot.Common
{
    public static class IEnumerableHelper
    {
        public static int Count(this IEnumerable source)
        {
            if (source is ICollection col)
                return col.Count;

            int c = 0;
            var e = source.GetEnumerator();
            DynamicUsing(e, () =>
            {
                while (e.MoveNext())
                    c++;
            });

            return c;
        }

        public static void DynamicUsing(object resource, Action action)
        {
            try
            {
                action();
            }
            finally
            {
                IDisposable d = resource as IDisposable;
                if (d != null)
                    d.Dispose();
            }
        }
    }
}

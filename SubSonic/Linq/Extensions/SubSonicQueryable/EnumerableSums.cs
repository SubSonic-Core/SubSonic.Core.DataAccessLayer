using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Linq
{
    public static partial class SubSonicQueryable
    {
        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return Enumerable.Sum(source, selector);
        }

        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return Enumerable.Sum(source, selector);
        }
    }
}

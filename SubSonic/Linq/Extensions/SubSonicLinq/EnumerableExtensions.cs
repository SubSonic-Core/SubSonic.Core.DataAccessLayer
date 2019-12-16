using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    /// <summary>
    /// Extension Method Forwarding to avoid name conflicts
    /// </summary>
    public static partial class SubSonicLinqExtensions
    {
        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> sources)
        {
            return Enumerable.ToArray(sources);
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if(source.IsNotNull() && source.IsSubSonicQuerable())
            {
                return Where(source.AsQueryable(), predicate);
            }
            return Enumerable.Where(source, predicate.IsNullThrowArgumentNull(nameof(predicate)).Compile());
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Enumerable.Any(source, predicate);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return Enumerable.Select(source, selector);
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            return Enumerable.ElementAt(source, index);
        }
    }
}

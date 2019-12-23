using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    /// <summary>
    /// Extension Method Forwarding to avoid name conflicts
    /// </summary>
    public static partial class SubSonicQueryable
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

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return Enumerable.SelectMany(source, selector);
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            return Enumerable.ElementAt(source, index);
        }        

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            return Enumerable.First(source);
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Enumerable.First(source, predicate);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return Enumerable.FirstOrDefault(source);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Enumerable.FirstOrDefault(source, predicate);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Microsoft set this standard")]
        public static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            return Enumerable.Single(source);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Microsoft set this standard")]
        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Enumerable.Single(source, predicate);
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return Enumerable.SingleOrDefault(source);
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Enumerable.SingleOrDefault(source, predicate);
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return Enumerable.Union(first, second);
        }
    }
}

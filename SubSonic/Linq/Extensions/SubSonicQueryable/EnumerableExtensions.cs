using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    /// <summary>
    /// Extension Method Forwarding to avoid name conflicts
    /// </summary>
    /// <remarks>
    /// I have had some time to think about this and this approach is valid, but may not be the rabbit hole I am looking for.
    /// I believe a better approach would be to map Iqueryable Extension methods in the Query Provider and SubSonicQueryable should only have the SubSonic Only Query Extensions.
    /// </remarks>
    public static partial class SubSonicQueryable
    {
        public static IEnumerable<TSource> ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (TSource item in source)
            {
                action(item);
            }

            return source;
        }

        public static bool SequenceEqual<TType>(this IEnumerable<TType> left, IEnumerable<TType> right)
        {
            return Enumerable.SequenceEqual(left, right);
        }

        public static bool SequenceEqual<TType>(this IEnumerable<TType> left, IEnumerable<TType> right, IEqualityComparer<TType> comparer)
        {
            return Enumerable.SequenceEqual(left, right, comparer);
        }

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            return Enumerable.Count(source);
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Enumerable.Count(source, predicate);
        }

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

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            return Enumerable.Skip(source, count);
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            return Enumerable.Take(source, count);
        }

        public static IEnumerable<TResult> Select<TResult>(this IEnumerable source, Func<object, TResult> selector)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            List<TResult> result = new List<TResult>();

            foreach(object item in source)
            {
                result.Add(selector(item));
            }

            return result;
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

        public static TResult SingleOrDefault<TSource, TResult>(this IEnumerable<TSource> source)
        {
            TSource single = Enumerable.SingleOrDefault(source);

            if ((object)single is TResult result)
            {
                return result;
            }

            return default(TResult);
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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace SubSonic.Linq
{
    public static class SubSonicEnumerable
    {
        //public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate)
        //{
        //    return Enumerable.Where(source, predicate.IsNullThrowArgumentNull(nameof(predicate)).Compile());
        //}

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

        public static TResult SingleOrDefault<TSource, TResult>(this IEnumerable<TSource> source)
        {
            TSource single = Enumerable.SingleOrDefault(source);

            if ((object)single is TResult result)
            {
                return result;
            }

            return default(TResult);
        }
    }
}

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
        public static IQueryable<TSource> ForEach<TSource>(this IQueryable<TSource> source, Action<TSource> action)
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
    }
}

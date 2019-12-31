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
    public static partial class SubSonicQueryable
    {
        public static List<TSource> ToList<TSource>(this IQueryable<TSource> source)
        {
            return Enumerable.ToList(source.Load());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    using Expressions;
    using Infrastructure;
    using System.Runtime.CompilerServices;

    public static partial class SubSonicQueryable
    {
        public static IOrderedEnumerable<TEntity> OrderBy<TEntity, TProperty>(this IEnumerable<TEntity> source, Func<TEntity, TProperty> selector)
        {
            return Enumerable.OrderBy(source, selector);
        }

        public static IOrderedEnumerable<TEntity> OrderBy<TEntity, TProperty>(this IEnumerable<TEntity> source, Func<TEntity, TProperty> selector, IComparer<TProperty> comparer)
        {
            return Enumerable.OrderBy(source, selector, comparer);
        }

        public static IOrderedQueryable<TEntity> OrderBy<TEntity, TProperty>(this IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> selector)
        {
            return Queryable.OrderBy(source, selector);
        }

        public static IOrderedEnumerable<TEntity> OrderByDescending<TEntity, TProperty>(this IEnumerable<TEntity> source, Func<TEntity, TProperty> selector)
        {
            return Enumerable.OrderByDescending(source, selector);
        }

        public static IOrderedEnumerable<TEntity> OrderByDescending<TEntity, TProperty>(this IEnumerable<TEntity> source, Func<TEntity, TProperty> selector, IComparer<TProperty> comparer)
        {
            return Enumerable.OrderByDescending(source, selector, comparer);
        }

        public static IOrderedQueryable<TEntity> OrderByDescending<TEntity, TProperty>(this IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> selector)
        {
            return Queryable.OrderByDescending(source, selector);
        }

        public static IOrderedEnumerable<TEntity> ThenBy<TEntity, TProperty>(this IOrderedEnumerable<TEntity> source, Func<TEntity, TProperty> selector)
        {
            return Enumerable.ThenBy(source, selector);
        }

        public static IOrderedEnumerable<TEntity> ThenBy<TEntity, TProperty>(this IOrderedEnumerable<TEntity> source, Func<TEntity, TProperty> selector, IComparer<TProperty> comparer)
        {
            return Enumerable.ThenBy(source, selector, comparer);
        }

        public static IOrderedEnumerable<TEntity> ThenByDescending<TEntity, TProperty>(this IOrderedEnumerable<TEntity> source, Func<TEntity, TProperty> selector)
        {
            return Enumerable.ThenByDescending(source, selector);
        }

        public static IOrderedEnumerable<TEntity> ThenByDescending<TEntity, TProperty>(this IOrderedEnumerable<TEntity> source, Func<TEntity, TProperty> selector, IComparer<TProperty> comparer)
        {
            return Enumerable.ThenByDescending(source, selector, comparer);
        }

        public static IOrderedQueryable<TEntity> ThenOrderBy<TEntity, TProperty>(this IOrderedQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> selector)
        {
            return Queryable.ThenBy(source, selector);
        }

        public static IOrderedQueryable<TEntity> ThenOrderByDescending<TEntity, TProperty>(this IOrderedQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> selector)
        {
            return Queryable.ThenByDescending(source, selector);
        }
    }
}

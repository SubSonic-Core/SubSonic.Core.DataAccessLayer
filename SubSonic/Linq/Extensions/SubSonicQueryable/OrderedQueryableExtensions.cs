using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    using Expressions;
    using Infrastructure;

    public static partial class SubSonicQueryable
    {
        public static IOrderedQueryable<TEntity> OrderBy<TEntity, TProperty>(this IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> selector)
        {
            return Queryable.OrderBy(source, selector);
        }

        public static IOrderedQueryable<TEntity> OrderByDescending<TEntity, TProperty>(this IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> selector)
        {
            return Queryable.OrderByDescending(source, selector);
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

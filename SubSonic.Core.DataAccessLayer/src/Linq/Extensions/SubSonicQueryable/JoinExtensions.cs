using System;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    using Expressions;
    using SubSonic.src;
    using SubSonic.src.Collections;
    using System.Reflection;
    using System.Threading;

    public static partial class SubSonicQueryable
    {
        public static IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(this IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> selector)
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
            if (selector is null)
            {
                throw Error.ArgumentNull(nameof(selector));

            }

            Type 
                entityType = typeof(TEntity),
                propertyType = typeof(TProperty);

            MethodInfo method = typeof(SubSonicQueryable).GetGenericMethod(nameof(Include), 
                new[] { entityType, propertyType }, 
                typeof(IQueryable<TEntity>),
                typeof(Expression<Func<TEntity, TProperty>>));

            return new IncludableSubSonicCollection<TEntity, TProperty>(
                source.Provider,
                source.Provider.CreateQuery<TEntity>(Expression.Call(method, source.Expression, selector)).Expression);
        }

        public static IIncludableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(this IIncludableQueryable<TEntity, TPreviousProperty> source, Expression<Func<TPreviousProperty, TProperty>> selector)
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
            if (selector is null)
            {
                throw Error.ArgumentNull(nameof(selector));

            }

            Type
                entityType = typeof(TEntity),
                previousType = typeof(TPreviousProperty),
                propertyType = typeof(TProperty);

            MethodInfo method = typeof(SubSonicQueryable).GetGenericMethod(nameof(ThenInclude),
                new[] { entityType, previousType, propertyType },
                typeof(IIncludableQueryable<TEntity, TPreviousProperty>),
                typeof(Expression<Func<TPreviousProperty, TProperty>>));

            return new IncludableSubSonicCollection<TEntity, TProperty>(
                source.Provider,
                source.Provider.CreateQuery<TEntity>(Expression.Call(method, source.Expression, selector)).Expression);
        }

        public static IIncludableQueryable<TEntity, TProperty> IncludeOptional<TEntity, TProperty>(this IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> selector)
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
            if (selector is null)
            {
                throw Error.ArgumentNull(nameof(selector));

            }

            Type
                entityType = typeof(TEntity),
                propertyType = typeof(TProperty);

            MethodInfo method = typeof(SubSonicQueryable).GetGenericMethod(nameof(IncludeOptional),
                new[] { entityType, propertyType },
                typeof(IQueryable<TEntity>),
                typeof(Expression<Func<TEntity, TProperty>>));

            return new IncludableSubSonicCollection<TEntity, TProperty>(
                source.Provider,
                source.Provider.CreateQuery<TEntity>(Expression.Call(method, source.Expression, selector)).Expression);
        }

        public static IIncludableQueryable<TEntity, TProperty> ThenIncludeOptional<TEntity, TPreviousProperty, TProperty>(this IIncludableQueryable<TEntity, TPreviousProperty> source, Expression<Func<TPreviousProperty, TProperty>> selector)
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
            if (selector is null)
            {
                throw Error.ArgumentNull(nameof(selector));

            }

            Type
                entityType = typeof(TEntity),
                previousType = typeof(TPreviousProperty),
                propertyType = typeof(TProperty);

            MethodInfo method = typeof(SubSonicQueryable).GetGenericMethod(nameof(ThenInclude),
                new[] { entityType, previousType, propertyType },
                typeof(IIncludableQueryable<TEntity, TPreviousProperty>),
                typeof(Expression<Func<TPreviousProperty, TProperty>>));

            return new IncludableSubSonicCollection<TEntity, TProperty>(
                source.Provider,
                source.Provider.CreateQuery<TEntity>(Expression.Call(method, source.Expression, selector)).Expression);
        }

        public static IQueryable<TSource> CrossApply<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.CrossApply, right);
        }

        public static IQueryable<TSource> CrossJoin<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.CrossJoin, right);
        }

        public static IQueryable<TSource> OuterApply<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.OuterApply, right);
        }

        internal static IQueryable<TSource> Join<TSource>(this IQueryable<TSource> source, JoinType type, DbExpression right)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                ISubSonicQueryProvider builder = (ISubSonicQueryProvider)query.Provider;

                if (query.Expression is DbSelectExpression select)
                {
                    if (builder.BuildJoin(type, select, right) is DbJoinExpression join)
                    {
                        return builder.CreateQuery<TSource>(DbExpression.DbSelect(select, join));
                    }
                }
            }

            return source;
        }
    }
}

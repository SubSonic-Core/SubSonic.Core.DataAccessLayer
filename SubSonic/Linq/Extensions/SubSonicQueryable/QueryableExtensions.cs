using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    using Expressions;
    using Infrastructure;
    using SubSonic.Infrastructure.Builders;
    using SubSonic.Infrastructure.Schema;
    using System.Diagnostics;
    using System.Reflection;

    public static partial class SubSonicQueryable
    {
        public static bool IsSubSonicQuerable<TSource>(this IEnumerable<TSource> source)
        {
            return source is ISubSonicCollection<TSource>;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static IDbPageCollection<TEntity> ToPagedCollection<TEntity>(this IQueryable<TEntity> source, int pageSize)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TEntity> query = source
                    .Page(default(int), pageSize)
                    .AsQueryable();

                ISubSonicQueryProvider builder = (ISubSonicQueryProvider)query.Provider;

                return builder
                    .ToPagedQuery(query.Expression, pageSize)
                    .ToPagedCollection<TEntity>();
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// ensure a queryable is loaded then send it to a array
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TSource[] ToArray<TSource>(this IQueryable<TSource> source)
        {
            return source.Load().AsEnumerable().ToArray();
        }

        /// <summary>
        /// ensure a queryable is loaded then send it to a list
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<TSource> ToList<TSource>(this IQueryable<TSource> source)
        {
            return source.Load().AsEnumerable().ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> Load<TEntity>(this IQueryable<TEntity> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query.Provider.Execute<IQueryable<TEntity>>(query.Expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="number"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int number, int size)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                ISubSonicQueryProvider builder = (ISubSonicQueryProvider)query.Provider;

                if (query.Expression is DbSelectExpression select)
                {
                    return builder.CreateQuery<TSource>(builder.BuildSelect(select, number, size));
                }
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Equivelent to SQL Select table.*
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<TSource> Select<TSource>(this IQueryable<TSource> source)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                ISubSonicQueryProvider builder = (ISubSonicQueryProvider)query.Provider;

                Expression where = null;

                if (query.Expression is DbSelectExpression select)
                {
                    where = select.Where;
                }

                return builder.CreateQuery<TSource>(builder.BuildSelect(query.Expression, where));
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IQueryable Select<TSource>(this IQueryable<TSource> source, IDbEntityProperty property)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                ISubSonicQueryProvider provider = (ISubSonicQueryProvider)source.Provider;

                return provider.CreateQuery(provider.BuildSelect(source.Expression, property));
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        public static IQueryable<TSource> WhereExists<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, IQueryable>> select)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                if (select is null)
                {
                    throw Error.ArgumentNull(nameof(select));
                }

                if (source.Provider is ISubSonicQueryProvider provider)
                {
                    MethodInfo method = typeof(SubSonicQueryable).GetGenericMethod(nameof(WhereExists), new[] { source.Expression.Type, select.GetType() });

                    return source.Provider.CreateQuery<TSource>(
                        provider.BuildSelect(source.Expression, 
                        DbWherePredicateBuilder.GetWhereTranslation(
                            DbExpression.DbWhere(method, new[] { source.Expression, select }))));
                }
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        public static IQueryable<TSource> WhereNotExists<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, IQueryable>> select)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                if (select is null)
                {
                    throw Error.ArgumentNull(nameof(select));
                }

                if (source.Provider is ISubSonicQueryProvider provider)
                {
                    MethodInfo method = typeof(SubSonicQueryable).GetGenericMethod(nameof(WhereNotExists), new[] { source.Expression.Type, select.GetType() });

                    return source.Provider.CreateQuery<TSource>(
                        provider.BuildSelect(source.Expression,
                        DbWherePredicateBuilder.GetWhereTranslation(
                            DbExpression.DbWhere(method, new[] { source.Expression, select }))));
                }
            }

            throw new NotSupportedException();
        }
    }
}

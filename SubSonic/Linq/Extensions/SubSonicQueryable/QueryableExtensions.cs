﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    using Expressions;
    using Infrastructure;

    public static partial class SubSonicQueryable
    {
        public static bool IsSubSonicQuerable<TSource>(this IEnumerable<TSource> source)
        {
            return source is ISubSonicCollection<TSource>;
        }
        public static IQueryable<TEntity> Load<TEntity>(this IQueryable<TEntity> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query.Provider.Execute<IQueryable<TEntity>>(query.Expression);
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

                ISubSonicQueryProvider<TSource> builder = DbContext.ServiceProvider.GetService<ISubSonicQueryProvider<TSource>>();

                Expression where = null;

                if (query.Expression is DbSelectExpression select)
                {
                    where = select.Where;
                }

                return builder.CreateQuery<TSource>(builder.BuildSelect(query.Expression, where));
            }

            throw new NotSupportedException();
        }

        public static IQueryable<TResult> Select<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IDbSqlQueryBuilderProvider provider = (IDbSqlQueryBuilderProvider)source.Provider;

                return provider.CreateQuery<TResult>(provider.BuildSelect(source.Expression, selector));
            }
            return Queryable.Select(source, selector);
        }

        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                if (source.Expression is DbSelectExpression query)
                {
                    ISubSonicQueryProvider<TSource> provider = (ISubSonicQueryProvider<TSource>)source.Provider;

                    Expression where = null;

                    if (source.Expression is ConstantExpression)
                    {
                        where = provider.BuildWhere(provider.GetDbTable(), null, source.GetType(), predicate);
                    }
                    else if (source.Expression is DbSelectExpression select)
                    {
                        where = provider.BuildWhere(select.From, select.Where, source.GetType(), predicate);
                    }

                    return (ISubSonicCollection<TSource>)provider.CreateQuery<TSource>(provider.BuildSelect(source.Expression, where));
                }
            }
            return Queryable.Where(source, predicate);
        }

        public static IQueryable<TSource> WhereExists<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, IQueryable>> select)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                if (select is null)
                {
                    throw new ArgumentNullException(nameof(select));
                }

                ISubSonicQueryProvider<TSource> provider = (ISubSonicQueryProvider<TSource>)source.Provider;

                Expression where = null;

                if (source.Expression is DbTableExpression)
                {
                    where = provider.BuildWhereExists(provider.GetDbTable(), source.GetType(), select);
                }
                else if (source.Expression is DbSelectExpression _select)
                {
                    where = provider.BuildWhereExists(_select.From, source.GetType(), select);
                }

                return (ISubSonicCollection<TSource>)provider.CreateQuery<TSource>(provider.BuildSelect(source.Expression, where));
            }

            throw new NotSupportedException();
        }

        public static IQueryable<TSource> WhereNotExists<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, IQueryable>> select)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                ISubSonicQueryProvider<TSource> provider = (ISubSonicQueryProvider<TSource>)source.Provider;

                Expression where = null;

                if (source.Expression is DbTableExpression)
                {
                    where = provider.BuildWhereNotExists(provider.GetDbTable(), source.GetType(), select);
                }
                else if (source.Expression is DbSelectExpression _select)
                {
                    where = provider.BuildWhereNotExists(_select.From, source.GetType(), select);
                }

                return (ISubSonicCollection<TSource>)provider.CreateQuery<TSource>(provider.BuildSelect(source.Expression, where));
            }
            throw new NotSupportedException();
        }

        public static TSource First<TSource>(this IQueryable<TSource> source)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Enumerable.First(query.Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.First(source);
        }

        public static TSource First<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Enumerable.First(query.Where(predicate).Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.First(source, predicate);
        }

        public static TSource FirstOrDefault<TSource>(this IQueryable<TSource> source)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Enumerable.FirstOrDefault(query.Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.FirstOrDefault(source);
        }

        public static TSource FirstOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Enumerable.FirstOrDefault(query.Where(predicate).Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.FirstOrDefault(source, predicate);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Microsoft already named a IQueryable.Single and it would be confusing not to.")]
        public static TSource Single<TSource>(this IQueryable<TSource> source)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Enumerable.Single(query.Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.Single(source);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Microsoft set this standard")]
        public static TSource Single<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Enumerable.Single(query.Where(predicate).Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.Single(source, predicate);
        }

        public static TSource SingleOrDefault<TSource>(this IQueryable<TSource> source)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Enumerable.SingleOrDefault(query.Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.SingleOrDefault(source);
        }

        public static TSource SingleOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Enumerable.SingleOrDefault(query.Where(predicate).Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.SingleOrDefault(source, predicate);
        }
    }
}
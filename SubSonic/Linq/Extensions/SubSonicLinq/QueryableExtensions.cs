using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    using Expressions;
    using Infrastructure;

    public static partial class SubSonicLinqExtensions
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

                if (query.Expression is DbSelectExpression)
                {
                    DbSelectExpression select = (DbSelectExpression)query.Expression;

                    where = select.Where;
                }
                else if (query.Expression is DbTableExpression)
                {
                    return builder.CreateQuery<TSource>(builder.BuildSelect());
                }
                return builder.CreateQuery<TSource>(builder.BuildSelect(where));
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
                ISubSonicQueryProvider<TSource> provider = (ISubSonicQueryProvider<TSource>)source.Provider;

                Expression where = null;

                if (source.Expression is DbTableExpression)
                {
                    where = provider.BuildWhere((DbTableExpression)provider.GetAliasedTable(), null, source.GetType(), predicate);
                }
                else if (source.Expression is DbSelectExpression)
                {
                    DbSelectExpression select = source.Expression as DbSelectExpression;

                    where = provider.BuildWhere((DbTableExpression)select.From, select.Where, source.GetType(), predicate);
                }

                return (ISubSonicCollection<TSource>)provider.CreateQuery<TSource>(provider.BuildSelect(source.Expression, where));
            }
            return Queryable.Where(source, predicate);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Microsoft already named a IQueryable.Single and it would be confusing not to.")]
        public static TSource Single<TSource>(this IQueryable<TSource> source)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                return Queryable.Single(query.Provider.Execute<IQueryable<TSource>>(query.Expression));
            }

            return Queryable.Single(source);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq
{
    using Infrastructure;
    using Infrastructure.Schema;
    using Expressions;

    public static partial class SubSonicExtensions
    {
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
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ISubSonicCollection<TEntity> Select<TEntity>(this ISubSonicCollection<TEntity> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            ISubSonicQueryProvider<TEntity> builder = DbContext.ServiceProvider.GetService<ISubSonicQueryProvider<TEntity>>();

            Expression where = null;

            if(query.Expression is DbSelectExpression)
            {
                DbSelectExpression select = (DbSelectExpression)query.Expression;

                where = select.Where;
            }
            else if (query.Expression is DbTableExpression)
            {
                return (ISubSonicCollection<TEntity>)builder.CreateQuery<TEntity>(builder.BuildSelect());
            }
            return (ISubSonicCollection<TEntity>)builder.CreateQuery<TEntity>(builder.BuildSelect(where));
        }

        public static ISubSonicCollection<TEntity> Select<TEntity, TColumn>(this ISubSonicCollection<TEntity> query, Expression<Func<TEntity, TColumn>> selector)
        {
            if (query.IsNotNull())
            {
                IDbSqlQueryBuilderProvider provider = (IDbSqlQueryBuilderProvider)query.Provider;

                return (ISubSonicCollection<TEntity>)provider.CreateQuery<TEntity>(provider.BuildSelect(query.Expression, selector));
            }
            return query;
        }

        public static ISubSonicCollection<TEntity> Where<TEntity>(this ISubSonicCollection<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            if (query.IsNotNull())
            {
                ISubSonicQueryProvider<TEntity> provider = (ISubSonicQueryProvider<TEntity>)query.Provider;

                Expression where = null;

                if (query.Expression is DbTableExpression)
                {
                    where = provider.BuildWhere((DbTableExpression)provider.GetAliasedTable(), query.GetType(), predicate);
                }
                else if (query.Expression is DbSelectExpression)
                {
                    DbSelectExpression select = query.Expression as DbSelectExpression;

                    where = provider.BuildWhere((DbTableExpression)select.From, query.GetType(), predicate);
                }

                return (ISubSonicCollection<TEntity>)provider.CreateQuery<TEntity>(provider.BuildSelect(query.Expression, where));
            }
            return query;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Microsoft already named a IQueryable.Single and it would be confusing not to.")]
        public static TEntity Single<TEntity>(this ISubSonicCollection<TEntity> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).Single();
        }
    }
}

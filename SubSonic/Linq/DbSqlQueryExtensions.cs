using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Linq
{
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
        public static IQueryable<TEntity> Select<TEntity>(this IQueryable<TEntity> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            IDbEntityModel model = DbContext.DbModel.GetEntityModel<TEntity>();

            return query.Provider.CreateQuery<TEntity>(
                    new DbSelectExpression(model.ToAlias(), query.Expression)
                    .SetColumns(model.Properties.ToColumnList((DbAliasedExpression)query.Expression)));
        }
    }
}

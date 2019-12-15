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
            return (ISubSonicCollection<TEntity>)builder.CreateQuery<TEntity>(builder.BuildSelect(null, where));
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

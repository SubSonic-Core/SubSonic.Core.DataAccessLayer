using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic
{
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
    }
}

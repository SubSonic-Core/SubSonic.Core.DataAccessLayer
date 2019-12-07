using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic
{
    public static partial class Extensions
    {
        public static IQueryable<TEntity> Load<TEntity>(this IQueryable<TEntity> query)
        {
            return query.Provider.Execute<IQueryable<TEntity>>(query.Expression);
        }
    }
}

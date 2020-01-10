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
        public static IQueryable<TSource> Join<TSource>(this IQueryable<TSource> source, JoinType type, DbExpression right)
        {
            if (source.IsNotNull() && source.IsSubSonicQuerable())
            {
                IQueryable<TSource> query = source.AsQueryable();

                IDbSqlQueryBuilderProvider builder = (IDbSqlQueryBuilderProvider)query.Provider;

                return builder.CreateQuery<TSource>(builder.BuildJoin(type, query.Expression, right));
            }

            return source;
        }
    }
}

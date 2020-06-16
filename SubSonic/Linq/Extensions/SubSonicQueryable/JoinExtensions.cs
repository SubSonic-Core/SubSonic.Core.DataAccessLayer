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
        public static IQueryable<TSource> CrossApply<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.CrossApply, right);
        }

        public static IQueryable<TSource> CrossJoin<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.CrossJoin, right);
        }

        public static IQueryable<TSource> InnerJoin<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.InnerJoin, right);
        }

        public static IQueryable<TSource> LeftOuter<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.LeftOuter, right);
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

                IDbSubSonicQueryProvider builder = (IDbSubSonicQueryProvider)query.Provider;

                return builder.CreateQuery<TSource>(builder.BuildJoin(type, query.Expression, right));
            }

            return source;
        }
    }
}

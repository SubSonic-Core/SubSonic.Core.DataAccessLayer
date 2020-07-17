using System;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    using Expressions;


    public static partial class SubSonicQueryable
    {
        public static IQueryable<TSource> Include<TSource, TEntity>(this IQueryable<TSource> source, Expression<Func<TSource, TEntity>> selector)
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
            if (selector is null)
            {
                throw Error.ArgumentNull(nameof(selector));

            }

            return source.Join(JoinType.InnerJoin, SubSonicContext.DbModel.GetEntityModel<TEntity>().Table);
        }

        public static IQueryable<TSource> CrossApply<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.CrossApply, right);
        }

        public static IQueryable<TSource> CrossJoin<TSource>(this IQueryable<TSource> source, DbExpression right)
        {
            return source.Join(JoinType.CrossJoin, right);
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

                ISubSonicQueryProvider builder = (ISubSonicQueryProvider)query.Provider;

                return builder.CreateQuery<TSource>(builder.BuildJoin(type, query.Expression, right));
            }

            return source;
        }
    }
}

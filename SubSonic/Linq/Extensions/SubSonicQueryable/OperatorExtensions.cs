using System.Linq;

namespace SubSonic.Linq
{
    public static partial class SubSonicQueryable
    {
        public static bool IsNull<TType>(this TType source)
        {
            return source == null;
        }

        public static bool IsNotNull<TType>(this TType source)
        {
            return !IsNull(source);
        }

        public static bool In<TType>(this TType source, params TType[] values)
        {
            return Enumerable.Any(values, (value) => value.Equals(source));
        }
        /// <summary>
        /// only used to represent a DbInExpression in code
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static bool In<TType, TEntity>(this TType source, IQueryable<TEntity> queryable)
        {
            return true;
        }

        public static bool NotIn<TType>(this TType source, params TType[] values)
        {
            return !Enumerable.Any(values, (value) => value.Equals(source));
        }
        /// <summary>
        /// only used to represent a DbNotInExpression in code
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static bool NotIn<TType, TEntity>(this TType source, IQueryable<TEntity> queryable)
        {
            return false;
        }
    }
}

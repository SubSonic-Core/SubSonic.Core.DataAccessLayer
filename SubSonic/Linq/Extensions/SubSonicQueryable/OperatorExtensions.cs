using System;
using System.Linq;

namespace SubSonic.Linq
{
    public static partial class SubSonicQueryable
    {
        public static bool IsNull<TType>(this TType source)
        {
            return source == null;
        }

        public static TType IsNull<TType>(this TType? source, TType @default = default(TType))
            where TType: struct
        {
            if (!source.HasValue)
            {
                return @default;
            }
            return source.Value;
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
        /// <param name="select"></param>
        /// <returns></returns>
        public static bool In<TType>(this TType source, IQueryable<TType> select)
        {
            return In(source, select.ToArray());
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
        /// <param name="select"></param>
        /// <returns></returns>
        public static bool NotIn<TType>(this TType source, IQueryable<TType> select)
        {
            return NotIn(source, select.ToArray());
        }

        public static bool Between(this DateTime value, DateTime start, DateTime end)
        {
            return (start < value) && (value < end); 
        }

        public static bool NotBetween(this DateTime value, DateTime start, DateTime end)
        {
            return !Between(value, start, end);
        }
    }
}

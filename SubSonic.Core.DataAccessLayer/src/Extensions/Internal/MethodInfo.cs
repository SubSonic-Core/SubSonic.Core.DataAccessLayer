using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic
{
    using Linq;
    using Linq.Expressions;
    using Legacy = System.Linq.Queryable;

    internal static partial class InternalExtensions
    {
        public static bool IsOrderBy(this MethodInfo info, out OrderByType orderByType)
        {
            orderByType = OrderByType.Ascending;

            if (info.Name.In(nameof(Legacy.OrderByDescending), nameof(Legacy.ThenByDescending)))
            {
                orderByType = OrderByType.Descending;
            }

            return info.Name.In(nameof(Legacy.OrderBy), nameof(Legacy.ThenBy), nameof(Legacy.OrderByDescending), nameof(Legacy.ThenByDescending));
        }

        public static bool IsWhere(this MethodInfo info)
        {
            return info.Name.Equals(nameof(Legacy.Where), StringComparison.CurrentCulture);
        }

        public static bool IsTake(this MethodInfo info)
        {
            return info.Name.Equals(nameof(Legacy.Take), StringComparison.CurrentCulture);
        }

        public static bool IsSkip(this MethodInfo info)
        {
            return info.Name.Equals(nameof(Legacy.Skip), StringComparison.CurrentCulture);
        }

        public static bool IsDistinct(this MethodInfo info)
        {
            return info.Name.Equals(nameof(Legacy.Distinct), StringComparison.CurrentCulture);
        }

        public static bool IsSupportedSelect(this MethodInfo info)
        {
            return info.GetParameters().Length > 1 &&
                info.Name.Equals(nameof(Legacy.Select), StringComparison.CurrentCulture) &&
                info.GetParameters()[1].ParameterType.IsSubclassOf(typeof(Expression));                   
        }

        public static bool IsSupportedIncludable(this MethodInfo info)
        {
            return info.GetParameters().Length > 1 &&
#if NETSTANDARD2_1
               info.Name.Contains(nameof(SubSonicQueryable.Include), StringComparison.CurrentCulture) &&
#elif NETSTANDARD2_0
               info.Name.Contains(nameof(SubSonicQueryable.Include)) &&
#endif
               info.GetParameters()[1].ParameterType.IsSubclassOf(typeof(Expression));
        }
    }
}

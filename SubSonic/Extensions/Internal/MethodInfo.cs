using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SubSonic.Linq.Expressions;

namespace SubSonic
{
    using Linq;
    
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
    }
}

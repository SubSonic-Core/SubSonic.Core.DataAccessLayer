using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && typeof(Nullable).IsAssignableFrom(type);
        }

        public static Type GetUnderlyingType(this Type type)
        {
            return type.IsNullableType() ? Nullable.GetUnderlyingType(type) : type;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Extensions.Test
{
    internal static partial class InternalExtenstions
    {
        public static bool IsNull<TType>(this TType instance)
        {
            return instance == null;
        }
        public static bool IsNotNull<TType>(this TType instance)
        {
            return instance != null;
        }
    }
}

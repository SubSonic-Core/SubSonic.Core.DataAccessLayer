using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
        public static bool IsNull(this object source)
        {
            return source == null;
        }

        public static bool IsNotNull(this object source)
        {
            return !IsNull(source);
        }
    }
}

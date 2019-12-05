using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// warning: some methods in this extension class are referenced at runtime and the reference count at design time will not reflect this.
    /// </remarks>
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

        public static TReturn IsNotNull<TType, TReturn>(this TType source, Func<TType, TReturn> selector, TReturn @default = default(TReturn))
        {
            if (IsNotNull(source))
            {
                return selector(source);
            }
            return @default;
        }

        public static TType IsNullThrow<TType>(this TType source, Exception exception)
        {
            if (IsNull(source))
            {
                throw exception;
            }
            return source;
        }
    }
}

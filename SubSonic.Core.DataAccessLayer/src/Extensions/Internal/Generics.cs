using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
#if NETSTANDARD2_0
        internal static bool TryPeek<TType>(this Stack<TType> stack, out TType result)
            where TType: class
        {
            result = null;

            try
            {
                result = stack.Peek();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            { }

            return !(result is null);
        }
#endif
    }
}

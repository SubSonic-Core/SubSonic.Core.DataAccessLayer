using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
        public static TType GetService<TType>(this IServiceProvider provider)
            where TType : class
        {
            return (TType)provider?.GetService(typeof(TType));
        }
    }
}

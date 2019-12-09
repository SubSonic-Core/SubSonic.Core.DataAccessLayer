using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Extensions.Test
{ 
    internal static partial class InternalExtenstions
    {
        public static TType GetService<TType>(this IServiceProvider provider)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return (TType)provider.GetService(typeof(TType));
        }
    }
}

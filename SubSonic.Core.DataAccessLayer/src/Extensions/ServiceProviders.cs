using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    using Linq;

    public static partial class SubSonicExtensions
    {
        public static TReturn GetService<TType, TReturn>(this IServiceProvider provider)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return (TReturn)provider.GetService(typeof(TType));
        }

        public static bool GetService<TType>(this IServiceProvider provider, out TType service)
        {
            return GetService<TType, TType>(provider, out service);
        }

        public static bool GetService<TType, TReturn>(this IServiceProvider provider, out TReturn service)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            service = (TReturn)provider.GetService(typeof(TType));

            return service.IsNotNull();
        }
    }
}

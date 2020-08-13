using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    using Linq;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;

    public static partial class SubSonicExtensions
    {
        public static DbContextOptionsBuilder ConfigureServiceCollection(this DbContextOptionsBuilder builder, IServiceCollection services = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            IServiceCollection collection = services ?? new ServiceCollection();
            if (!Enumerable.Any<ServiceDescriptor>(collection, delegate (ServiceDescriptor service) {
                return service.ServiceType == typeof(IServiceCollection);
            }))
            {
                collection.AddSingleton<IServiceCollection, ServiceCollection>(delegate (IServiceProvider provider) {
                    return (ServiceCollection)collection;
                });
            }
            builder.SetServiceProvider(collection.BuildServiceProvider());
            return builder;
        }

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

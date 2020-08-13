using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace SubSonic
{
    using SubSonic.Linq;
    using SubSonic.Logging;

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

        public static DbContextOptionsBuilder AddLogging(this DbContextOptionsBuilder builder, Action<ILoggingBuilder> configure)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            IServiceCollection services = builder.ServiceProvider.GetService<IServiceCollection>();

            services.AddLogging(configure);

            builder.ConfigureServiceCollection(services);

            return builder;
        }

        public static ILoggingBuilder AddDebugLogger<TClassName>(this ILoggingBuilder builder)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            builder.AddProvider(new DebugLogProvider<TClassName>());
#pragma warning restore CA2000 // Dispose objects before losing scope

            return builder;
        }

        public static ILoggingBuilder AddTraceLogger<TClassName>(this ILoggingBuilder builder)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            builder.AddProvider(new TraceLogProvider<TClassName>());
#pragma warning restore CA2000 // Dispose objects before losing scope

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

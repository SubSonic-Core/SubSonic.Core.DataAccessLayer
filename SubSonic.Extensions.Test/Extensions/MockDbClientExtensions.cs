using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubSonic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SubSonic.Extensions.Test
{
    using SqlServer.SqlQueryProvider;

    public static partial class SubSonicTestExtensions
    {
        public static DbContextOptionsBuilder ConfigureServiceCollection(this DbContextOptionsBuilder builder, IServiceCollection services = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            SubSonicRigging.Services = services ?? new ServiceCollection();

            if (!SubSonicRigging.Services.Any(service => service.ServiceType == typeof(IServiceCollection)))
            {
                SubSonicRigging.Services
                    .AddSingleton<IServiceCollection, ServiceCollection>(provider => (ServiceCollection)SubSonicRigging.Services);
            }

            builder.SetServiceProvider(SubSonicRigging.Services.BuildServiceProvider());

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

        public static DbContextOptionsBuilder UseMockDbClient(this DbContextOptionsBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Type providerFactoryType = typeof(MockDbClient.MockDbClientFactory);

            builder
                .RegisterProviderFactory(providerFactoryType.FullName, providerFactoryType)
                .RegisterSqlQueryProvider(DbProviderInvariantNames.MockDbProviderInvariantName, typeof(MockSqlQueryProvider))
                .RegisterSqlQueryProvider(DbProviderInvariantNames.SqlServiceDbProviderInvariantName, typeof(SqlServerSqlQueryProvider))
                .SetDefaultProviderFactory(providerFactoryType.FullName);

            IServiceCollection services = builder.ServiceProvider.GetService<IServiceCollection>();

            if (services.IsNotNull())
            {
                services.AddSingleton(DbProviderFactories.GetFactory(builder.Options.ProviderInvariantName));
            }

            return builder;
        }
    }
}

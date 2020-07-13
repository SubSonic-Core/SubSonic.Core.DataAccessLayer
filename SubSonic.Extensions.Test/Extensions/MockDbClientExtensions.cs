using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubSonic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SubSonic.Extensions.Test
{
    using SqlServer;

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

        public static DbContextOptionsBuilder UseMockDbClient(this DbContextOptionsBuilder builder, Action<DbConnectionStringBuilder, SubSonicContextOptions> config = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Type providerFactoryType = typeof(SubSonicMockDbClient);

            builder
                .RegisterProviderFactory(DbProviderInvariantNames.MockDbProviderInvariantName, providerFactoryType)
                .SetDefaultProvider(DbProviderInvariantNames.MockDbProviderInvariantName, DbProviderInvariantNames.SqlServiceDbProviderInvariantName)
                .SetConnectionStringBuilder(config);

            return builder;
        }
    }
}

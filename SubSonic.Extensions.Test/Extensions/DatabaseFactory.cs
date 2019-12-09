using Microsoft.Extensions.DependencyInjection;
using SubSonic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Extensions.Test
{
    public static partial class SubSonicExtensions
    {
        public static DbContextOptionsBuilder ConfigureServiceCollection(this DbContextOptionsBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            SubSonicRigging.Services = new ServiceCollection();

            SubSonicRigging.Services.AddSingleton(SubSonicRigging.Services);

            builder.SetServiceProvider(SubSonicRigging.Services.BuildServiceProvider());

            return builder;
        }

        public static DbContextOptionsBuilder UseMockDbProviderFactory(this DbContextOptionsBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Type providerFactoryType = typeof(MockDbProvider.MockDbProviderFactory);

            builder
                .RegisterProviderFactory(providerFactoryType.FullName, providerFactoryType)
                .SetDefaultProviderFactory(providerFactoryType.FullName);

            IServiceCollection services = builder.ServiceProvider.GetService<IServiceCollection>();

            if (services.IsNotNull())
            {
                services.AddSingleton(DbProviderFactories.GetFactory(providerFactoryType.FullName));
            }

            return builder;
        }
    }
}

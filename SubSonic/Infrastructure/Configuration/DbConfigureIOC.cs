using Microsoft.Extensions.DependencyInjection;
using SubSonic.Infrastructure.Builders;
using SubSonic.Infrastructure.Logging;
using SubSonic.Infrastructure.Providers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal static class DbConfigureIOC
    {
        public static void SetupIOC(this DbContext context, IServiceCollection services, DbContextOptions options)
        {
            services
                    .AddSingleton(context)
                    .AddScoped(provider => DbProviderFactories.GetFactory(options.DbProviderInvariantName))
                    .AddScoped(provider => SqlQueryProviderFactory.GetProvider(options.SqlQueryProviderInvariantName))
                    .AddScoped(typeof(ISubSonicLogger<>), typeof(SubSonicLogger<>))
                    .AddScoped(typeof(ISubSonicDbSetCollectionProvider<>), typeof(SubSonicDbSetCollectionProvider<>))
                    .AddScoped(typeof(DbSetCollection<>))
                    .AddScoped<DbDatabase>()
                    //.AddScoped(typeof(DbSqlQueryBuilder<>)) not needed yet
                    .AddScoped<SharedDbConnectionScope>()
                    .AddScoped<AutomaticConnectionScope>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using SubSonic.Infrastructure.Builders;
using SubSonic.Infrastructure.Logging;
using System.Data.Common;

namespace SubSonic.Infrastructure
{
    using Schema;
    using System.Linq;

    internal static class DbConfigureExtensions
    {
        public static void SetupIOC(this DbContext context, IServiceCollection services, DbContextOptions options)
        {
            services
                    .AddSingleton(context)
                    .AddTransient(provider => DbProviderFactories.GetFactory(options.DbProviderInvariantName))
                    .AddTransient(provider => SqlQueryProviderFactory.GetProvider(options.SqlQueryProviderInvariantName))
                    .AddScoped(typeof(ISubSonicLogger<>), typeof(SubSonicLogger<>))
                    .AddScoped(typeof(ISubSonicLogger), typeof(SubSonicLogger<DbContext>))
                    .AddTransient(typeof(ISubSonicQueryProvider<>), typeof(DbSqlQueryBuilder<>))
                    .AddTransient(typeof(DbSetCollection<>))
                    .AddScoped<DbDatabase>()
                    .AddTransient<SharedDbConnectionScope>()
                    .AddTransient<AutomaticConnectionScope>();
        }

        public static void PreCompile(this DbContext context)
        {
            foreach (IDbEntityModel model in context.Model.EntityModels)
            {
                DbSqlQueryBuilder builder = new DbSqlQueryBuilder(model.EntityModelType);
            }
        }
    }
}

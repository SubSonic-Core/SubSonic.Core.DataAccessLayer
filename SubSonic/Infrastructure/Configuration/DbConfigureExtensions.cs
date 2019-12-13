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
                    .AddScoped(provider => DbProviderFactories.GetFactory(options.DbProviderInvariantName))
                    .AddScoped(provider => SqlQueryProviderFactory.GetProvider(options.SqlQueryProviderInvariantName))
                    .AddScoped(typeof(ISubSonicLogger<>), typeof(SubSonicLogger<>))
                    .AddScoped(typeof(ISubSonicLogger), typeof(SubSonicLogger<DbContext>))
                    .AddScoped(typeof(ISubSonicQueryProvider<>), typeof(DbSqlQueryBuilder<>))
                    .AddScoped(typeof(DbSetCollection<>))
                    .AddScoped<DbDatabase>()
                    .AddScoped<SharedDbConnectionScope>()
                    .AddScoped<AutomaticConnectionScope>();
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

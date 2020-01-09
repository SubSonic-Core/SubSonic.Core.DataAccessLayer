using Microsoft.Extensions.DependencyInjection;
using SubSonic.Infrastructure.Builders;
using SubSonic.Infrastructure.Logging;
using System.Data.Common;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    using Schema;
    using Data.DynamicProxies;
    using Infrastructure.Factory;
    using Data.Caching;

    internal static class DbConfigureExtensions
    {
        public static void SetupIOC(this DbContext context, IServiceCollection services, DbContextOptions options)
        {
            services
                    .AddSingleton(context)
                    .AddSingleton(new ChangeTrackerCollection())
                    .AddTransient(provider => DbProviderFactories.GetFactory(options.DbProviderInvariantName))
                    .AddTransient(provider =>
                    {
                        if (DbProviderFactories.GetFactory(options.DbProviderInvariantName) is SubSonicDbProvider client)
                        {
                            return client.QueryProvider;
                        }
                        throw new NotSupportedException();
                    })
                    .AddScoped<DbContextAccessor>()
                    .AddScoped(typeof(ISubSonicLogger<>), typeof(SubSonicLogger<>))
                    .AddScoped(typeof(ISubSonicLogger), typeof(SubSonicLogger<DbContext>))
                    .AddTransient(typeof(ISubSonicQueryProvider<>), typeof(DbSqlQueryBuilder<>))
                    .AddTransient(typeof(DbSetCollection<>))
                    .AddScoped<DbDatabase>()
                    .AddTransient<SharedDbConnectionScope>()
                    .AddTransient<AutomaticConnectionScope>();
        }

        private static void Compile(Expression<Func<IQueryable>> query)
        {
            Expression.Lambda(query).Compile().DynamicInvoke();
        }

        public static void PreCompile(this DbContext context)
        {
            foreach (IDbEntityModel model in context.Model.EntityModels)
            {
                DbSqlQueryBuilder builder = new DbSqlQueryBuilder(model.EntityModelType);

                Compile(() => new SubSonicCollection(model.EntityModelType));
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using SubSonic.Builders;
using SubSonic.Logging;
using System.Data.Common;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic
{
    using Collections;
    using Schema;
    using Data.DynamicProxies;
    using Factory;
    using Data.Caching;

    internal static class SubSonicConfigureExtensions
    {
        public static void SetupIOC(this SubSonicContext context, IServiceCollection services, SubSonicContextOptions options)
        {
            services
                    .AddScoped(typeof(ISubSonicLogger<>), typeof(SubSonicLogger<>))
                    .AddScoped(typeof(ISubSonicLogger), typeof(SubSonicLogger<SubSonicContext>))
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
                    .AddTransient(typeof(ISubSonicQueryProvider<>), typeof(DbSqlQueryBuilder<>))
                    .AddTransient(typeof(SubSonicSetCollection<>))
                    .AddTransient(typeof(ISubSonicSetCollection<>), typeof(SubSonicSetCollection<>))
                    .AddScoped<DbDatabase>()
                    .AddTransient<SharedDbConnectionScope>()
                    .AddTransient<AutomaticConnectionScope>();
        }

        private static void Compile(Expression<Func<IQueryable>> query)
        {
            Expression.Lambda(query).Compile().DynamicInvoke();
        }

        public static void PreCompile(this SubSonicContext context)
        {
            foreach (IDbEntityModel model in context.Model.EntityModels)
            {
                DbSqlQueryBuilder builder = new DbSqlQueryBuilder(model.EntityModelType);

                Compile(() => new SubSonicCollection(model.EntityModelType));
            }
        }
    }
}

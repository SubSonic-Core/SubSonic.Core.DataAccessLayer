using SubSonic.Data.Caching;
using SubSonic.Data.DynamicProxies;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Factory;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic
{
    public partial class SubSonicContext
    {
        internal static SubSonicSchemaModel DbModel => ServiceProvider.GetService<SubSonicContext>().IsNotNull(Ctx => Ctx.Model);
        internal static SubSonicContextOptions DbOptions => ServiceProvider.GetService<SubSonicContext>().IsNotNull(Ctx => Ctx.Options);
        internal static IServiceProvider ServiceProvider { get; set; }
        internal Func<DbConnectionStringBuilder, SubSonicContextOptions, string> GetConnectionString { get; set; }

        protected internal static object CreateObject(Type type)
        {
            if (DbOptions.EnableProxyGeneration)
            {
                DynamicProxyWrapper proxy = DynamicProxy.GetProxyWrapper(type);

                return Activator.CreateInstance(proxy.Type, ServiceProvider.GetService<DbContextAccessor>());
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }

        internal static string GenerateSqlFor(Expression query)
        {
            if(ServiceProvider.GetService<DbProviderFactory>() is SubSonicDbProvider client)
            {
                return client.QueryProvider.GenerateSqlFor(query);
            }
            throw new NotSupportedException();
        }
    }
}

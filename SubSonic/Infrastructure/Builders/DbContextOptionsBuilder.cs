using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbContextOptionsBuilder
    {
        private readonly DbContext dbContext;
        private readonly DbContextOptions options;

        private bool isDirtyServiceProvider = false;

        public DbContextOptionsBuilder(DbContext dbContext, DbContextOptions options)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void EnableProxyGeneration()
        {
            options.EnableProxyGeneration = true;
        }

        public static void RegisterDataProvider(string providerInvariantName, DbProviderFactory factory)
        {
            DbProviderFactories.RegisterFactory(providerInvariantName, factory);
        }

        public static void RegisterDataProvider(string providerInvariantName, Type factoryType)
        {
            DbProviderFactories.RegisterFactory(providerInvariantName, factoryType);
        }

        public static void RegisterDataProvider(string providerInvariantName, string factoryTypeAssembyQualifiedName)
        {
            DbProviderFactories.RegisterFactory(providerInvariantName, factoryTypeAssembyQualifiedName);
        }

        public void SetServiceProvider(IServiceProvider provider)
        {
            if (!isDirtyServiceProvider)
            {
                dbContext.Instance = provider;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SubSonic
{
    public class DbContextOptionsBuilder
    {
        private readonly SubSonicContext dbContext;
        private readonly SubSonicContextOptions options;

        private bool isDirtyServiceProvider;

        internal class ConnectionStringKeys
        {
            public const string Async = "Async";
            public const string MARS = "MultipleActiveResultSets";
            public const string IntegratedSecurity = "Integrated Security";
            public const string FailOverPartner = "Failover Partner";
            public const string ConnectionTimeout = "Connection Timeout";
            public const string AttachDBFilename = "AttachDBFilename";
            public const string ConnectionLifetime = "Connection Lifetime";
            public const string DataSource = "Data Source";
            public const string Encrypt = "Encrypt";
            public const string InitialCatalog = "Initial Catalog";
            public const string LoadBalanceTimeout = "Load Balance Timeout";
            public const string MaxPoolSize = "Max Pool Size";
            public const string MinPoolSize = "Min Pool Size";
            public const string ContextConnection = "Context Connection";
            public const string PacketSize = "Packet Size";
            public const string Password = "Password";
            public const string PersistSecurityInfo = "Persist Security Info";
            public const string Pooling = "Pooling";
            public const string UserID = "User ID";
            public const string WorkstationID = "Workstation  ID";
        }

        public DbContextOptionsBuilder(SubSonicContext dbContext, SubSonicContextOptions options)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            this.options.EnableProxyGeneration = true;
        }

        public SubSonicContextOptions Options => options;

        public IServiceProvider ServiceProvider => dbContext.Instance;

        public DbContextOptionsBuilder SetConnectionStringBuilder(Action<DbConnectionStringBuilder, SubSonicContextOptions> connection)
        {
            dbContext.GetConnectionString = (builder, options) =>
            {
                connection(builder, options);

                return builder.ConnectionString;
            };

            return this;
        }

        public DbContextOptionsBuilder SetDefaultProvider(string dbProviderInvariantName, string sqlQueryProviderInvariantName = null)
        {
            if (string.IsNullOrEmpty(dbProviderInvariantName))
            {
                throw new ArgumentException("", nameof(dbProviderInvariantName));
            }

            sqlQueryProviderInvariantName = sqlQueryProviderInvariantName ?? dbProviderInvariantName;

            if (!DbProviderFactories.GetProviderInvariantNames().Any(provider => provider.Equals(dbProviderInvariantName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ProviderInvariantNameNotRegisteredException(dbProviderInvariantName, typeof(DbProviderFactories).Name);
            }

            options.DbProviderInvariantName = dbProviderInvariantName;
            options.SqlQueryProviderInvariantName = sqlQueryProviderInvariantName;

            return this;
        }

        public DbContextOptionsBuilder EnableMultipleActiveResultSets(bool enable)
        {
            options.UseMultipleActiveResultSets = enable;

            return this;
        }

        public DbContextOptionsBuilder RegisterProviderFactory(string providerInvariantName, DbProviderFactory factory)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentException("", nameof(providerInvariantName));
            }

            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            DbProviderFactories.RegisterFactory(providerInvariantName, factory);

            return this;
        }

        public DbContextOptionsBuilder RegisterProviderFactory(string providerInvariantName, Type factoryType)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentException("", nameof(providerInvariantName));
            }

            if (factoryType is null)
            {
                throw new ArgumentNullException(nameof(factoryType));
            }

            DbProviderFactories.RegisterFactory(providerInvariantName, factoryType);

            return this;
        }

        public DbContextOptionsBuilder RegisterProviderFactory(string providerInvariantName, string factoryTypeAssembyQualifiedName)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentException("", nameof(providerInvariantName));
            }

            if (string.IsNullOrEmpty(factoryTypeAssembyQualifiedName))
            {
                throw new ArgumentException("", nameof(factoryTypeAssembyQualifiedName));
            }

            DbProviderFactories.RegisterFactory(providerInvariantName, factoryTypeAssembyQualifiedName);

            return this;
        }

        public DbContextOptionsBuilder SetServiceProvider(IServiceProvider provider)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (!isDirtyServiceProvider)
            {
                SubSonicContext.ServiceProvider = provider;
            }

            return this;
        }
    }
}


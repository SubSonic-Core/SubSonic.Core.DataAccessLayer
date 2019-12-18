using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace SubSonic.Extensions.Test
{
    using Data.Builders;
    using Infrastructure;
    using Infrastructure.Schema;
    using MockDbClient;
    using System.Data;

    public static partial class SubSonicTestExtensions
    {
        public static void AddCommandBehavior<TEntity>(this MockDbClientFactory factory, string command, IEnumerable<TEntity> entities)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException("", nameof(command));
            }

            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            IDbEntityModel model = DbContext.DbModel.GetEntityModel<TEntity>();

            using (DataTableBuilder table = new DataTableBuilder(model.Name))
            {
                foreach(IDbEntityProperty property in model.Properties)
                {
                    table.AddColumn(property.Name, property.PropertyType);
                }

                foreach(TEntity entity in entities)
                {
                    DataRow row = table.CreateRow();

                    foreach (IDbEntityProperty property in model.Properties)
                    {
                        table.AddColumn(property.Name, property.PropertyType);
                    }
                }
            }
        }
        public static void UpdateProviders(this DbContext dbContext, string dbProviderInvariantName, string sqlQueryProviderInvariantName = null)
        {
            if (dbContext is null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (string.IsNullOrEmpty(dbProviderInvariantName))
            {
                throw new ArgumentException("", nameof(dbProviderInvariantName));
            }

            sqlQueryProviderInvariantName = sqlQueryProviderInvariantName ?? dbProviderInvariantName;

            if (!DbProviderFactories.GetProviderInvariantNames().Any(provider => provider.Equals(dbProviderInvariantName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ProviderInvariantNameNotRegisteredException(dbProviderInvariantName, typeof(DbProviderFactories).Name);
            }

            if (!SqlQueryProviderFactory.GetProviderInvariantNames().Any(provider => provider.Equals(sqlQueryProviderInvariantName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ProviderInvariantNameNotRegisteredException(sqlQueryProviderInvariantName, typeof(SqlQueryProviderFactory).Name);
            }

            dbContext.Options.SetDbProviderInvariantName(dbProviderInvariantName);
            dbContext.Options.SetSqlQueryProviderInvariantName(sqlQueryProviderInvariantName);
        }
        public static void UpdateConnectionString(this DbContext dbContext, Action<DbConnectionStringBuilder, DbContextOptions> config)
        {
            if (dbContext is null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            dbContext.GetConnectionString = (builder, options) =>
            {
                config(builder, options);

                return builder.ConnectionString;
            };
        }
    }
}

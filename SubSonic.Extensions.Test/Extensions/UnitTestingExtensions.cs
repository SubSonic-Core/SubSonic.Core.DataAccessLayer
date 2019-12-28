using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SubSonic.Extensions.Test
{
    using Data.Builders;
    using Extensions.Test.MockDbClient.Syntax;
    using Infrastructure;
    using Infrastructure.Schema;
    using Linq;

    public static partial class SubSonicTestExtensions
    {
        public static string GetSql<TEntity>(this ICollection<TEntity> entities)
        {
            if (entities is null)
            {
                return "";
            }

            if (entities.IsSubSonicQuerable())
            {
                return ((IQueryable<TEntity>)entities).Expression.ToString();
            }

            return "";
        }

        public static string GetSql<TEntity>(this IEnumerable<TEntity> entities)
        {
            if (entities is null)
            {
                return "";
            }

            if (entities.IsSubSonicQuerable())
            {
                return ((IQueryable<TEntity>)entities).Expression.ToString();
            }

            return "";
        }

        public static string GetSql<TEntity>(this IQueryable<TEntity> entities)
        {
            if (entities is null)
            {
                return "";
            }

            if (entities.IsSubSonicQuerable())
            {
                return (entities).Expression.ToString();
            }

            return "";
        }

        public static void AddCommandBehavior<TEntity>(this DbProviderFactory factory, string command, IEnumerable<TEntity> entities)
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
                foreach (IDbEntityProperty property in model.Properties)
                {
                    if (property.EntityPropertyType == DbEntityPropertyType.Value)
                    {
                        table.AddColumn(property.Name, property.PropertyType);
                    }
                }

                foreach(TEntity entity in entities)
                {
                    DataRow row = table.CreateRow();

                    foreach (IDbEntityProperty property in model.Properties)
                    {
                        if (property.EntityPropertyType == DbEntityPropertyType.Value)
                        {
                            row[property.Name] = model.EntityModelType
                                .GetProperty(property.PropertyName)
                                .GetValue(entity) ?? DBNull.Value;
                        }
                    }

                    table.AddRow(row);
                }

                if (factory is SubSonicMockDbClient db)
                {
                    db.AddBehavior(new MockCommandBehavior()
                        .When((cmd) => cmd.CommandText == command)
                        .ReturnsData(table.DataTable));
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

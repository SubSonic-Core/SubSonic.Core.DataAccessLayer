using System;
using System.Data.Common;

namespace SubSonic.Extensions
{
    public static partial class BuilderExtensions
    {
        public static DbConnectionStringBuilder SetAsync(this DbConnectionStringBuilder builder, bool enabled = false)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.Async, enabled);

            return builder;
        }

        public static DbConnectionStringBuilder SetIntegratedSecurity(this DbConnectionStringBuilder builder, bool enabled = false)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.IntegratedSecurity, enabled);

            return builder;
        }

        public static DbConnectionStringBuilder SetUserCredentials(this DbConnectionStringBuilder builder, string userId, string password)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("", nameof(userId));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("", nameof(password));
            }

            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.UserID, userId);
            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.Password, password);

            return builder;
        }

        public static DbConnectionStringBuilder SetDatasource(this DbConnectionStringBuilder builder, string datasource)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(datasource))
            {
                throw new ArgumentException("", nameof(datasource));
            }

            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.DataSource, datasource);

            return builder;
        }

        public static DbConnectionStringBuilder SetInitialCatalog(this DbConnectionStringBuilder builder, string catalog)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(catalog))
            {
                throw new ArgumentException("", nameof(catalog));
            }

            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.InitialCatalog, catalog);

            return builder;
        }

        public static DbConnectionStringBuilder SetConnectionTimeout(this DbConnectionStringBuilder builder, int timeoutInSeconds)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.ConnectionTimeout, timeoutInSeconds);

            return builder;
        }

        public static DbConnectionStringBuilder SetPersistSecurityInfo(this DbConnectionStringBuilder builder, bool persisted = false)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.PersistSecurityInfo, persisted);

            return builder;
        }

        public static DbConnectionStringBuilder SetFailOverPartner(this DbConnectionStringBuilder builder, string value)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("", nameof(value));
            }

            builder.Add(DbContextOptionsBuilder.ConnectionStringKeys.FailOverPartner, value);

            return builder;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SubSonic.Extensions.SqlServer
{
    using Infrastructure;
    using System.Data.Common;

    public static partial class SqlServerExtensions
    {
        public static DbContextOptionsBuilder UseSqlClient(this DbContextOptionsBuilder builder, Action<DbConnectionStringBuilder, DbContextOptions> config = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Type providerFactoryType = typeof(SqlClientFactory);

            string providerInvariantName = providerFactoryType.Namespace;

            builder
                .RegisterProviderFactory(providerInvariantName, providerFactoryType)
                .RegisterSqlQueryProvider(providerInvariantName, typeof(SqlServerSqlQueryProvider))
                .SetDefaultProvider(providerInvariantName)
                .SetConnectionStringBuilder(config);

            return builder;
        }
    }
}

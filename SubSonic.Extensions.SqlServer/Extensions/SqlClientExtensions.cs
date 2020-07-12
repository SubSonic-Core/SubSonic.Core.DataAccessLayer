using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SubSonic.Extensions.SqlServer
{
    using Infrastructure;
    using System.Data;
    using System.Data.Common;

    public static partial class SqlServerExtensions
    {
        public static DbContextOptionsBuilder UseSqlClient(this DbContextOptionsBuilder builder, Action<DbConnectionStringBuilder, SubSonicContextOptions> config = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Type providerFactoryType = typeof(SubSonicSqlClient);

            string providerInvariantName = providerFactoryType.Namespace;

            builder
                .RegisterProviderFactory(providerInvariantName, providerFactoryType)
                .SetDefaultProvider(providerInvariantName)
                .SetConnectionStringBuilder(config);

            return builder;
        }
    }
}

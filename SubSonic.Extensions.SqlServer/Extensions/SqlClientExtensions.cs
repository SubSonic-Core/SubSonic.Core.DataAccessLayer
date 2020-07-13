using System;
using System.Data.Common;

namespace SubSonic.Extensions.SqlServer
{
    

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

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Extensions.Test
{
    using Infrastructure;
    using System.Linq;

    public static partial class SubSonicTestExtensions
    {
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
    }
}

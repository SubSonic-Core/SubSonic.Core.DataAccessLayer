using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbContextOptions
    {
        public DbContextOptions()
        {
        }

        public bool EnableProxyGeneration { get; internal set; }
        public string DbProviderInvariantName { get; internal set; }
        public string SqlQueryProviderInvariantName { get; internal set; }
        public bool UseMultipleActiveResultSets { get; internal set; }

        internal void SetDbProviderInvariantName(string dbProviderInvariantName)
        {
            if (string.IsNullOrEmpty(dbProviderInvariantName))
            {
                throw new ArgumentException("", nameof(dbProviderInvariantName));
            }

            DbProviderInvariantName = dbProviderInvariantName;
        }

        internal void SetSqlQueryProviderInvariantName(string sqlQueryProviderInvariantName)
        {
            if (string.IsNullOrEmpty(sqlQueryProviderInvariantName))
            {
                throw new ArgumentException("", nameof(sqlQueryProviderInvariantName));
            }

            SqlQueryProviderInvariantName = sqlQueryProviderInvariantName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbContextOptions
    {
        public DbContextOptions()
        {
        }

        internal Action<DbSchemaModel> PreLoadHandler { get; set; }

        public bool EnableProxyGeneration { get; internal set; }
        public bool SupportUnicode { get; internal set; }
        public string DbProviderInvariantName { get; internal set; }
        public string SqlQueryProviderInvariantName { get; internal set; }
        public bool UseMultipleActiveResultSets { get; internal set; }

        public void SetDbProviderInvariantName(string dbProviderInvariantName)
        {
            if (string.IsNullOrEmpty(dbProviderInvariantName))
            {
                throw new ArgumentException("", nameof(dbProviderInvariantName));
            }

            DbProviderInvariantName = dbProviderInvariantName;
        }

        public void SetSqlQueryProviderInvariantName(string sqlQueryProviderInvariantName)
        {
            if (string.IsNullOrEmpty(sqlQueryProviderInvariantName))
            {
                throw new ArgumentException("", nameof(sqlQueryProviderInvariantName));
            }

            SqlQueryProviderInvariantName = sqlQueryProviderInvariantName;
        }
    }
}

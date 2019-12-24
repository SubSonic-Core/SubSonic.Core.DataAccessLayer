using System.Collections.Generic;
using System.Data.Common;

namespace SubSonic.Infrastructure
{
    internal class DbStoredProcedure
    {
        public DbStoredProcedure(string sql, IEnumerable<DbParameter> parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new System.ArgumentException("", nameof(sql));
            }

            Sql = sql;
            Parameters = parameters ?? throw new System.ArgumentNullException(nameof(parameters));
        }
        public string Sql { get; }

        public IEnumerable<DbParameter> Parameters { get; }
    }
}

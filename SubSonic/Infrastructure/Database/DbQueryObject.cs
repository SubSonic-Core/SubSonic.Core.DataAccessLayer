using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbQueryObject
        : IDbQueryObject
    {
        public DbQueryObject(string sql, IReadOnlyCollection<SubSonicParameter> parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("", nameof(sql));
            }

            Sql = sql;
            Parameters = parameters ?? new ReadOnlyCollection<SubSonicParameter>(Array.Empty<SubSonicParameter>());
        }
        public string Sql { get; }
        public IReadOnlyCollection<SubSonicParameter> Parameters { get; }
    }
}

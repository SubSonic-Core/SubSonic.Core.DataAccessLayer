using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SubSonic.Infrastructure
{
    internal class DbStoredProcedure
        : IDbQuery
    {
        public DbStoredProcedure(string sql, string name, IEnumerable<DbParameter> parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new System.ArgumentException("", nameof(sql));
            }

            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            Sql = sql;
            Name = name;
            Parameters = new ReadOnlyCollection<DbParameter>(parameters.ToList());
        }
        public string Sql { get; }

        public string Name { get; }

        public IReadOnlyCollection<DbParameter> Parameters { get; }

        public CommandBehavior Behavior { get; set; }
    }
}

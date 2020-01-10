using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;

namespace SubSonic.Infrastructure
{
    internal class DbQueryObject
        : IDbQueryObject
    {
        public DbQueryObject(string sql)
            : this(sql, Array.Empty<DbParameter>())
        {

        }

        public DbQueryObject(string sql, params DbParameter[] parameters)
            : this(sql, CommandBehavior.Default, parameters)
        {
            
        }

        public DbQueryObject(string sql, CommandBehavior behavior, params DbParameter[] parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("", nameof(sql));
            }

            IList<DbParameter> _parameters = new List<DbParameter>(parameters);

            Sql = sql;
            Behavior = behavior;
            Parameters = new ReadOnlyCollection<DbParameter>(_parameters);
        }

        public CommandBehavior Behavior { get; }
        public string Sql { get; }
        public IReadOnlyCollection<DbParameter> Parameters { get; }
    }
}

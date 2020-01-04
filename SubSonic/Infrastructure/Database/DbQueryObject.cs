using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace SubSonic.Infrastructure
{
    internal class DbQueryObject
        : IDbQueryObject
    {
        public DbQueryObject(string sql)
            : this(sql, Array.Empty<SubSonicParameter>())
        {

        }

        public DbQueryObject(string sql, params IEnumerable<SubSonicParameter>[] parameters)
            : this(sql, CommandBehavior.Default, parameters)
        {
            
        }

        public DbQueryObject(string sql, CommandBehavior behavior, params IEnumerable<SubSonicParameter>[] parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("", nameof(sql));
            }

            IList<SubSonicParameter> _parameters = new List<SubSonicParameter>(parameters.SelectMany(p => p.IsNull(Array.Empty<SubSonicParameter>())));

            Sql = sql;
            Behavior = behavior;
            Parameters = new ReadOnlyCollection<SubSonicParameter>(_parameters);
        }

        public CommandBehavior Behavior { get; }
        public string Sql { get; }
        public IReadOnlyCollection<SubSonicParameter> Parameters { get; }
    }
}

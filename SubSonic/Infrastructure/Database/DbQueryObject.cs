using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SubSonic.Infrastructure
{
    internal class DbQueryObject
        : IDbQueryObject
    {
        public DbQueryObject(string sql, params IEnumerable<SubSonicParameter>[] parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("", nameof(sql));
            }

            IList<SubSonicParameter> _parameters = new List<SubSonicParameter>(parameters.SelectMany(p => p.IsNull(Array.Empty<SubSonicParameter>())));

            Sql = sql;
            Parameters = new ReadOnlyCollection<SubSonicParameter>(_parameters);
        }
        public string Sql { get; }
        public IReadOnlyCollection<SubSonicParameter> Parameters { get; }
    }
}

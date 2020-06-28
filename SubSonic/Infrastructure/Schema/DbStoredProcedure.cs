using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace SubSonic.Infrastructure
{
    internal class DbStoredProcedure
        : DbQuery
    {
        public DbStoredProcedure(string sql, string name, IEnumerable<DbParameter> parameters)
            : base(sql)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            Name = name;
            Parameters = new ReadOnlyCollection<DbParameter>(parameters.ToList());
        }

        public string Name { get; }

        public override IReadOnlyCollection<DbParameter> Parameters { get; }
    }
}

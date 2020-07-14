using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace SubSonic
{
    internal class DbQuery
        : IDbQuery
    {
        public DbQuery(string sql)
            : this(sql, Array.Empty<DbParameter>())
        {

        }

        public DbQuery(string sql, params DbParameter[] parameters)
            : this(sql, CommandBehavior.Default, parameters)
        {
            
        }

        public DbQuery(string sql, CommandBehavior behavior, params DbParameter[] parameters)
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

        protected DbQuery(CommandBehavior behavior)
        {
            Behavior = behavior;
        }

        public CommandBehavior Behavior { get; set; }
        public virtual string Sql { get; }
        public virtual IReadOnlyCollection<DbParameter> Parameters { get; }

        public virtual void CleanUpParameters()
        {
            //foreach (DbParameter parameter in Parameters)
            Parallel.ForEach(Parameters, parameter =>
            {
                if (parameter.Value is DataTable table)
                {
                    table.Dispose();
                    parameter.Value = null;
                }
            }
            );
        }
    }
}

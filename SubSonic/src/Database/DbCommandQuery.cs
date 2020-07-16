using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace SubSonic
{
    public class DbCommandQuery
    {
        public DbCommandQuery()
        {
            CommandType = CommandType.Text;
        }

        public DbCommandQuery(DbQueryType queryType)
            : this()
        {
            QueryType = queryType;
        }

        public DbCommandQuery(DbQueryType queryType, Type storedProcedureType)
            : this(queryType)
        {
            CommandType = CommandType.StoredProcedure;
            StoredProcedureType = storedProcedureType;
        }

        public DbQueryType QueryType { get; }

        [DefaultValue(CommandType.Text)]
        public CommandType CommandType { get; }

        public Type StoredProcedureType { get; }
    }
}

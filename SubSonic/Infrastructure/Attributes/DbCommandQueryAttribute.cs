using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DbCommandQueryAttribute
        : DbStoredProcedureAttribute
    {
        public DbCommandQueryAttribute(DbCommandQueryType queryType, string commandProcedureName) 
            : base(commandProcedureName)
        {
            QueryType = queryType;
        }

        public DbCommandQueryType QueryType { get; }
    }
}

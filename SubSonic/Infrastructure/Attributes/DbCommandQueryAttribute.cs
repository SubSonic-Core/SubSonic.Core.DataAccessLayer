using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DbCommandQueryAttribute
        : Attribute
    {
        public DbCommandQueryAttribute(DbCommandQueryType queryType, Type storedProcedureType) 
            : base()
        {
            QueryType = queryType;
            StoredProcedureType = storedProcedureType;
        }

        public DbCommandQueryType QueryType { get; }

        public Type StoredProcedureType { get; }
    }
}

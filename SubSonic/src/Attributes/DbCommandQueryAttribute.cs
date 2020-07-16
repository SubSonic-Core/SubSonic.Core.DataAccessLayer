using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DbCommandQueryAttribute
        : Attribute
    {
        public DbCommandQueryAttribute(DbQueryType queryType, Type storedProcedureType) 
            : base()
        {
            QueryType = queryType;
            StoredProcedureType = storedProcedureType;
        }

        public DbQueryType QueryType { get; }

        public Type StoredProcedureType { get; }
    }
}

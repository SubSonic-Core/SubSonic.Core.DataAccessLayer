using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DbStoredProcedureAttribute
        : DbProgrammabilityAttribute
    {
        public DbStoredProcedureAttribute(string procedureName)
            : base(procedureName, DbProgrammabilityType.StoredProcedure)
        {
        }
    }
}

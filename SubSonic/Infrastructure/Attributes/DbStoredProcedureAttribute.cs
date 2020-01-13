using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            CommandType = CommandType.StoredProcedure;
        }

        [DefaultValue(CommandType.Text)]
        public CommandType CommandType { get; set; }
    }
}

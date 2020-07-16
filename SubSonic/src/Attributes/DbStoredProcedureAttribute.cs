using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace SubSonic
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

        [DefaultValue(CommandType.StoredProcedure)]
        public CommandType CommandType { get; set; }

        [DefaultValue(false)]
        public bool IsNonQuery { get; set; }
    }
}

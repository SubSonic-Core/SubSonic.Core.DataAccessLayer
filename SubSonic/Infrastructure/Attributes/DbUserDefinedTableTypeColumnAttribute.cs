using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class DbUserDefinedTableTypeColumnAttribute
        : DbProgrammabilityAttribute
    {
        public DbUserDefinedTableTypeColumnAttribute(string name) 
            : base(name, DbProgrammabilityType.Column)
        {
        }

        public int Order { get; set; }

        public bool IsNullable { get; set; }

        public int DbType { get; set; }
    }
}

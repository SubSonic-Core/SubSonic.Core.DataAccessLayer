using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    using Schema;

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DbUserDefinedTableTypeAttribute
        : DbProgrammabilityAttribute
    {
        public DbUserDefinedTableTypeAttribute(string name) 
            : base(name, DbProgrammabilityType.Table)
        {
        }
    }
}

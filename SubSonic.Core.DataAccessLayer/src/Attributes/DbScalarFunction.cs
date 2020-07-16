using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SubSonic
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DbScalarFunctionAttribute
        : DbProgrammabilityAttribute
    {
        public DbScalarFunctionAttribute(string functionName)
            : base(functionName, DbProgrammabilityType.ScalarValuedFunction)
        {

        }
    }
}

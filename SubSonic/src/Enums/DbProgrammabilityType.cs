using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public enum DbProgrammabilityType
    {
        Unknown = 0,
        StoredProcedure,
        TableValuedFunction,
        ScalarValuedFunction,
        Parameter,
        Table,
        Column//,
        //AggregateFunction
    }
}

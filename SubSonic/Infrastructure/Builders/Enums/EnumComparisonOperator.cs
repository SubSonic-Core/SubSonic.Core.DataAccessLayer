using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal enum EnumComparisonOperator
    {
        Equal = 0,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Contains,
        NotContains,
        StartsWith,
        NotStartsWith,
        EndsWith,
        NotEndsWith,
        In,
        NotIn
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public enum DbComparisonOperator
    {
        Unknown = 0,
        Equal,
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
        NotIn,
        Between,
        NotBetween
    }
}

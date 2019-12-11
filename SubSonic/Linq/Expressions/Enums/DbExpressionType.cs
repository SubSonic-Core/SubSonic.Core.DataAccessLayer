using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    public enum DbExpressionType
    {
        Table = 1000, // make sure these don't overlap with ExpressionType
        ClientJoin,
        Column,
        Select,
        Projection,
        Join,
        Aggregate,
        Scalar,
        Exists,
        In,
        NotIn,
        Grouping,
        AggregateSubquery,
        IsNull,
        IsNotNull,
        Between,
        RowCount,
        NamedValue,
        OuterJoined,
        Limit,
        Function
    }
}

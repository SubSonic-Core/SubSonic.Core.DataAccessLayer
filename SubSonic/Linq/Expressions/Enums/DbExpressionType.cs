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
        Where,
        Update,
        Delete,
        Insert,
        Projection,
        Join,
        Aggregate,
        Scalar,
        Exists,
        NotExists,
        In,
        NotIn,
        Grouping,
        AggregateSubQuery,
        IsNull,
        IsNotNull,
        Between,
        NotBetween,
        RowCount,
        NamedValue,
        OuterJoined,
        Limit,
        Function
    }
}

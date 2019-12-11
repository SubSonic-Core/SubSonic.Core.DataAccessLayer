using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// A kind of SQL join
    /// </summary>
    public enum JoinType
    {
        CrossJoin,
        InnerJoin,
        CrossApply,
        OuterApply,
        LeftOuter
    }
}

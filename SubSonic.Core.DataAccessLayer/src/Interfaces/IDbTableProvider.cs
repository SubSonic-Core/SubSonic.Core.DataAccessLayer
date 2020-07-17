using SubSonic.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    internal interface ISubSonicTableProvider
    {
        IEnumerable<DbTableExpression> Tables { get; }
    }
}

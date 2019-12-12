using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Alias;
    using Infrastructure.SqlGenerator;
    using System.Reflection;

    public partial class TSqlFormatter
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            return base.VisitBinary(node);
        }
    }
}

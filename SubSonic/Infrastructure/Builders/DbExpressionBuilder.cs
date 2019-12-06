using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbExpressionBuilder
    {
        Expression pe;

        public DbExpressionBuilder(ParameterExpression expression)
        {
            pe = expression ?? throw new ArgumentNullException(nameof(expression));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    public abstract class DbExpressionAccessor
    {
        public DbExpressionAccessor()
        {
        }

        public static Expression GetMethodCall(Expression expression)
        {
            if (expression.IsNotNull())
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Call:
                        return (MethodCallExpression)expression;
                    default:
                        break;
                }
            }
            return null;
        }
    }
}

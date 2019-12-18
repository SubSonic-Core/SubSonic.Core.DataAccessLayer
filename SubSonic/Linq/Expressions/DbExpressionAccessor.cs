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

        public static Expression GetParameter(Expression expression)
        {
            if (expression.IsNotNull())
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        return (ParameterExpression)expression;
                    case (ExpressionType)DbExpressionType.Table:
                        return ((DbTableExpression)expression).Parameter;
                    default:
                        throw new NotSupportedException();
                }
            }
            return null;
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

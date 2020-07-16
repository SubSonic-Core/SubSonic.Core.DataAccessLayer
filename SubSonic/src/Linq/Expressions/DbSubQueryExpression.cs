using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public abstract class DbSubQueryExpression 
        : DbExpression
    {
        protected DbSubQueryExpression(DbExpressionType eType, Type type, Expression expression)
            : base(eType, type)
        {
            if (!(eType == DbExpressionType.Scalar || eType == DbExpressionType.In || eType == DbExpressionType.NotIn))
            {
                throw new InvalidOperationException();
            }

            Expression = expression;
        }

        public Expression Expression { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}

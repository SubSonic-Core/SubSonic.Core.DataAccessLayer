using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbScalarExpression : DbSubQueryExpression
    {
        protected internal DbScalarExpression(Type returnType, Expression expression, Expression[] arguments)
            : base(DbExpressionType.Scalar, returnType, expression)
        {
            Arguments = arguments;
        }

        public IEnumerable<Expression> Arguments { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitScalar(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbScalar(Type returnType, Expression expression, Expression[] arguments)
        {
            return new DbScalarExpression(returnType, expression, arguments);
        }
    }
}

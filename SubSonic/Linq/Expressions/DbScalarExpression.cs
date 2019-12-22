using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbScalarExpression : DbSubQueryExpression
    {
        protected internal DbScalarExpression(Type type, DbExpression expression)
            : base(DbExpressionType.Scalar, type, expression)
        {
        }

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
        public static DbExpression DbScalar(Type type, DbExpression expression)
        {
            return new DbScalarExpression(type, expression);
        }
    }
}

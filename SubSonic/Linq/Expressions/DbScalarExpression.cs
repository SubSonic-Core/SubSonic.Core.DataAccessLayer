using System;

namespace SubSonic.Linq.Expressions
{
    public class DbScalarExpression : DbSubQueryExpression
    {
        public DbScalarExpression(Type type, DbExpression expression)
            : base(DbExpressionType.Scalar, type, expression)
        {
        }
    }
}

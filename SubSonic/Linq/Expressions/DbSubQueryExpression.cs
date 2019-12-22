using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public abstract class DbSubQueryExpression 
        : DbExpression
    {
        protected DbSubQueryExpression(DbExpressionType eType, Type type, DbExpression select)
            : base(eType, type)
        {
            if (!(eType == DbExpressionType.Scalar || eType == DbExpressionType.In || eType == DbExpressionType.NotIn))
            {
                throw new InvalidOperationException();
            }

            Select = select;
        }

        public DbExpression Select { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}

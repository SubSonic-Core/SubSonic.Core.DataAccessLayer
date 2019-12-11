using System;

namespace SubSonic.Linq.Expressions
{
    public abstract class DbSubQueryExpression : DbExpression
    {
        protected DbSubQueryExpression(DbExpressionType eType, Type type, DbExpression select)
            : base(eType, type)
        {
            if (!(eType == DbExpressionType.Scalar || eType == DbExpressionType.Exists || eType == DbExpressionType.In || eType == DbExpressionType.NotIn))
            {
                throw new InvalidOperationException();
            }

            Select = select;
        }
        public DbExpression Select { get; }
    }
}

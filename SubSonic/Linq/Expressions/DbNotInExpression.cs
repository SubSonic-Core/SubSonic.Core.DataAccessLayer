using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbNotInExpression
        : DbInExpression
    {
        public DbNotInExpression(Expression expression, DbSelectExpression select)
            : base(DbExpressionType.NotIn, select)
        {
            Expression = expression;
        }
        public DbNotInExpression(Expression expression, NewArrayExpression array)
            : base(DbExpressionType.NotIn)
        {
            Expression = expression;
            Array = array;
        }

        public override Expression Expression { get; }
        public override NewArrayExpression Array { get; }
    }
}

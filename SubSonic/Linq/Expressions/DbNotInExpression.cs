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
        public DbNotInExpression(Expression expression, IEnumerable<Expression> values)
            : base(DbExpressionType.NotIn)
        {
            Expression = expression;
            Values = values as ReadOnlyCollection<Expression>;
            if (Values == null && values != null)
            {
                Values = new List<Expression>(values).AsReadOnly();
            }
        }

        public override Expression Expression { get; }
        public override ReadOnlyCollection<Expression> Values { get; }
    }
}

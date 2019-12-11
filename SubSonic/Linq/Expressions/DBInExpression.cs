using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbInExpression
        : DbSubQueryExpression
    {
        public DbInExpression(Expression expression, DbSelectExpression select)
            : base(DbExpressionType.In, typeof(bool), select)
        {
            Expression = expression;
        }
        public DbInExpression(Expression expression, IEnumerable<Expression> values)
            : base(DbExpressionType.In, typeof(bool), null)
        {
            Expression = expression;
            Values = values as ReadOnlyCollection<Expression>;
            if (Values == null && values != null)
            {
                Values = new List<Expression>(values).AsReadOnly();
            }
        }
        public Expression Expression { get; }
        public ReadOnlyCollection<Expression> Values { get; }
    }
}

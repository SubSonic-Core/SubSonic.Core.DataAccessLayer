using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbInExpression
        : DbSubQueryExpression
    {
        public DbInExpression(Expression expression, DbSelectExpression select)
            : this(DbExpressionType.In, select)
        {
            Expression = expression;
        }
        public DbInExpression(Expression expression, IEnumerable<Expression> values)
            : this(DbExpressionType.In)
        {
            Expression = expression;
            Values = values as ReadOnlyCollection<Expression>;
            if (Values == null && values != null)
            {
                Values = new List<Expression>(values).AsReadOnly();
            }
        }

        protected DbInExpression(DbExpressionType eType, DbExpression expression = null)
            : base(eType, typeof(bool), expression)
        {

        }

        public override Expression Expression { get; }
        public virtual ReadOnlyCollection<Expression> Values { get; }
    }
}

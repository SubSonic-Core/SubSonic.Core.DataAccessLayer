using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbBetweenExpression : DbExpression
    {
        public DbBetweenExpression(Expression expression, Expression lower, Expression upper)
            : base(DbExpressionType.Between, expression.IsNullThrowMissingArgument(nameof(expression)).Type)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Lower = lower ?? throw new ArgumentNullException(nameof(lower));
            Upper = upper ?? throw new ArgumentNullException(nameof(upper));
        }
        public Expression Expression { get; }
        public Expression Lower { get; }
        public Expression Upper { get; }
    }
}

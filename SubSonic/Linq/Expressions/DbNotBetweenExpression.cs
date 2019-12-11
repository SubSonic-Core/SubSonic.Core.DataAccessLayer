using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbNotBetweenExpression : DbExpression
    {
        public DbNotBetweenExpression(Expression expression, Expression lower, Expression upper)
            : base(DbExpressionType.NotBetween, expression.IsNullThrowArgumentNull(nameof(expression)).Type)
        {
            Expression = expression;
            Lower = lower ?? throw new ArgumentNullException(nameof(lower));
            Upper = upper ?? throw new ArgumentNullException(nameof(upper));
        }

        public override Expression Expression { get; }
        public virtual Expression Lower { get; }
        public virtual Expression Upper { get; }
    }
}

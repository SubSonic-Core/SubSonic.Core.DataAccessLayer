using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbOuterJoinedExpression : DbExpression
    {
        public DbOuterJoinedExpression(Expression test, Expression expression)
            : base(DbExpressionType.OuterJoined, expression.IsNullThrowArgumentNull(nameof(expression)).Type)
        {
            Test = test ?? throw new ArgumentNullException(nameof(test));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Expression Test { get; }

        public override Expression Expression { get; }
    }
}

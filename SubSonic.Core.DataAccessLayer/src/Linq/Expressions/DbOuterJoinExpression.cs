using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbOuterJoinedExpression 
        : DbExpression
    {
        protected internal DbOuterJoinedExpression(Expression test, Expression expression)
            : base(DbExpressionType.OuterJoined, expression.IsNullThrowArgumentNull(nameof(expression)).Type)
        {
            Test = test ?? throw new ArgumentNullException(nameof(test));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Expression Test { get; }

        public virtual Expression Expression { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitOuterJoined(this);
            }
            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbOuterJoined(Expression test, Expression expression)
        {
            return new DbOuterJoinedExpression(test, expression);
        }
    }
}

using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbNotBetweenExpression 
        : DbBetweenExpression
    {
        protected internal DbNotBetweenExpression(Expression value, Expression lower, Expression upper)
            : base(DbExpressionType.NotBetween)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Lower = lower ?? throw new ArgumentNullException(nameof(lower));
            Upper = upper ?? throw new ArgumentNullException(nameof(upper));
        }

        public override Expression Value { get; }
        public override Expression Lower { get; }
        public override Expression Upper { get; }
    }

    public partial class DbExpression
    {
        public static DbExpression DbNotBetween(Expression value, Expression lower, Expression upper)
        {
            return new DbNotBetweenExpression(value, lower, upper);
        }
    }
}

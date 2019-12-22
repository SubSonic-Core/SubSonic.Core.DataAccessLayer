using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbNotBetweenExpression : DbExpression
    {
        protected internal DbNotBetweenExpression(Expression value, Expression lower, Expression upper)
            : base(DbExpressionType.NotBetween, value.IsNullThrowArgumentNull(nameof(value)).Type)
        {
            Value = value;
            Lower = lower ?? throw new ArgumentNullException(nameof(lower));
            Upper = upper ?? throw new ArgumentNullException(nameof(upper));
        }

        public virtual Expression Value { get; }
        public virtual Expression Lower { get; }
        public virtual Expression Upper { get; }
    }

    public partial class DbExpression
    {
        public static DbExpression DbNotBetween(Expression value, Expression lower, Expression upper)
        {
            return new DbNotBetweenExpression(value, lower, upper);
        }
    }
}

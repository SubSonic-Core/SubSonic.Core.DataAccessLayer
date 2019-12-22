using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbBetweenExpression : DbExpression
    {
        protected internal DbBetweenExpression(Expression value, Expression lower, Expression upper)
            : this(DbExpressionType.Between, value.IsNullThrowArgumentNull(nameof(value)).Type)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Lower = lower ?? throw new ArgumentNullException(nameof(lower));
            Upper = upper ?? throw new ArgumentNullException(nameof(upper));
        }

        protected DbBetweenExpression(DbExpressionType eType, Type type)
            : base(eType, type) { }

        public virtual Expression Value { get; }
        public virtual Expression Lower { get; }
        public virtual Expression Upper { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitBetween(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbBetween(Expression value, Expression lower, Expression upper)
        {
            return new DbBetweenExpression(value, lower, upper);
        }
    }
}

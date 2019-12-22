using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    /// <summary>
    /// Allows is-null tests against value-types like int and float
    /// </summary>
    public class DbIsNullExpression
        : DbExpression
    {
        protected internal DbIsNullExpression(Expression expression)
            : this(DbExpressionType.IsNull)
        {
            Expression = expression;
        }

        protected DbIsNullExpression(DbExpressionType eType)
            : base(eType, typeof(bool)) { }

        public virtual Expression Expression { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if(visitor is DbExpressionVisitor db)
            {
                return db.VisitNull(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbIsNull(Expression expression)
        {
            return new DbIsNullExpression(expression);
        }

        public static DbExpression DbIsNull(DbExpressionType eType, Expression expression)
        {
            if (eType == DbExpressionType.IsNull)
            {
                return DbIsNull(expression);
            }
            else if (eType == DbExpressionType.IsNull)
            {
                return DbIsNotNull(expression);
            }

            throw new NotSupportedException();
        }
    }
}

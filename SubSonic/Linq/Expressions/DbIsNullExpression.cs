using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// Allows is-null tests against value-types like int and float
    /// </summary>
    public class DbIsNullExpression
        : DbExpression
    {
        public DbIsNullExpression(Expression expression)
            : this(DbExpressionType.IsNull)
        {
            Expression = expression;
        }

        protected DbIsNullExpression(DbExpressionType eType)
            : base(eType, typeof(bool)) { }

        public override Expression Expression { get; }
    }
}

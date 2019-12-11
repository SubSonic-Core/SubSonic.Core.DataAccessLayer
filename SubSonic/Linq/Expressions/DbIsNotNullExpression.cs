using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// Allows is-not-null tests against value-types like int and float
    /// </summary>
    public class DbIsNotNullExpression
        : DbIsNullExpression
    {
        public DbIsNotNullExpression(Expression expression)
            : base(DbExpressionType.IsNotNull)
        {
            Expression = expression;
        }

        public override Expression Expression { get; }
    }
}

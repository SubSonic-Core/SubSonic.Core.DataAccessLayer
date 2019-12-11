using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// Allows is-not-null tests against value-types like int and float
    /// </summary>
    public class DbIsNotNullExpression
        : DbExpression
    {
        public DbIsNotNullExpression(Expression expression)
            : base(DbExpressionType.IsNotNull, typeof(bool))
        {
            Expression = expression;
        }
        public Expression Expression { get; }
    }
}

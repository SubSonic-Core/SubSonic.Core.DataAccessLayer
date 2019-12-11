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
            : base(DbExpressionType.IsNull, typeof(bool))
        {
            Expression = expression;
        }
        public Expression Expression { get; }
    }
}

using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// Allows is-not-null tests against value-types like int and float
    /// </summary>
    public class DbIsNotNullExpression
        : DbIsNullExpression
    {
        protected internal DbIsNotNullExpression(Expression expression)
            : base(DbExpressionType.IsNotNull)
        {
            Expression = expression;
        }

        public override Expression Expression { get; }
    }

    public partial class DbExpression
    {
        public static DbExpression DbIsNotNull(Expression expression)
        {
            return new DbIsNotNullExpression(expression);
        }
    }
}

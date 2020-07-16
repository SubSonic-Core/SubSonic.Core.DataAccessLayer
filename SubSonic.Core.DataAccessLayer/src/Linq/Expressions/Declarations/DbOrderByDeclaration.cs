using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// A pairing of an expression and an order type for use in a SQL Order By clause
    /// </summary>
    public class DbOrderByDeclaration
    {
        public DbOrderByDeclaration(OrderByType orderType, Expression expression)
        {
            OrderByType = orderType;
            this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }
        public OrderByType OrderByType { get; }
        public Expression Expression { get; }
    }
}

using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbColumnDeclaration
    {
        public DbColumnDeclaration(string propertyName, int order, Expression expression)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("", nameof(propertyName));
            }

            PropertyName = propertyName;
            Order = order;
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }
        public string PropertyName { get; }
        public int Order { get; }
        public Expression Expression { get; }
    }
}

using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure.Schema;
    public class DbColumnDeclaration
    {
        private readonly IDbEntityProperty property;

        public DbColumnDeclaration(IDbEntityProperty property)
            : this(
                  property.IsNullThrowArgumentNull(nameof(property)).PropertyName,
                  property.IsNullThrowArgumentNull(nameof(property)).Order,
                  property.IsNullThrowArgumentNull(nameof(property)).Expression)
        {
            this.property = property;
        }
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
        public IDbEntityProperty Property => property;

        public string PropertyName { get; }
        public int Order { get; }
        public Expression Expression { get; }
    }
}

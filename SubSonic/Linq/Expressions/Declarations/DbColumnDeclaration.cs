using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbColumnDeclaration
    {
        public DbColumnDeclaration(string name, int order, Expression expression)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            Name = name;
            Order = order;
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }
        public string Name { get; }
        public int Order { get; }
        public Expression Expression { get; }
    }
}

using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbColumnDeclaration
    {
        string name;
        Expression expression;
        public DbColumnDeclaration(string name, Expression expression)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            this.name = name;
            this.expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }
        public string Name
        {
            get { return name; }
        }
        public Expression Expression
        {
            get { return expression; }
        }
    }
}

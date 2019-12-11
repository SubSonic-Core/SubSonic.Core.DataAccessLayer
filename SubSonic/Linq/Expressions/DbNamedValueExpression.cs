using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbNamedValueExpression
        : DbExpression
    {
        public DbNamedValueExpression(string name, Expression value)
            : base(DbExpressionType.NamedValue, value.IsNullThrowArgumentNull(nameof(value)).Type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            Name = name;
            Value = value;
        }
        public string Name { get; }
        public Expression Value { get; }
    }
}

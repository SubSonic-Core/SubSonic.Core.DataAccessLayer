using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;
    public class DbNamedValueExpression
        : DbExpression
    {
        protected internal DbNamedValueExpression(string name, Expression value)
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

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitExpression(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbNamedValue(string name, Expression value)
        {
            return new DbNamedValueExpression(name, value);
        }
    }
}

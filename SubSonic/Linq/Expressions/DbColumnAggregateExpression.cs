using System;
using System.Globalization;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbColumnAggregateExpression
        : DbExpression
    {
        protected internal DbColumnAggregateExpression(DbAggregateExpression expression, string name)
            : base(DbExpressionType.Column, expression.IsNullThrowArgumentNull(nameof(expression)).Type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            Argument = expression ?? throw new ArgumentNullException(nameof(expression));
            Name = name.ToUpper(CultureInfo.CurrentCulture).Trim();
        }

        public DbExpression Argument { get; }

        public string Name { get; }

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
        public static DbExpression DbColumnAggregate(DbExpression expression, string columnName)
        {
            if (expression is DbAggregateExpression aggregate)
            {
                return new DbColumnAggregateExpression(aggregate, columnName);
            }

            throw new NotSupportedException();
        }
    }
}

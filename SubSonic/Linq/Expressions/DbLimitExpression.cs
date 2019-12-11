using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbLimitExpression
        : DbExpression
    {
        public DbLimitExpression(int skip, int take, Expression value)
            : base(DbExpressionType.Limit, value.IsNullThrowMissingArgument(nameof(value)).Type)
        {
            Skip = skip;
            Take = take;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public int Skip { get; }
        public int Take { get; }
        public Expression Value { get; }
    }
}

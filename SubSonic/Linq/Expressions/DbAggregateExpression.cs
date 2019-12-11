using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbAggregateExpression
        : DbExpression
    {
        public DbAggregateExpression(Type type, AggregateType aggType, Expression argument, bool isDistinct)
            : base(DbExpressionType.Aggregate, type)
        {
            AggregateType = aggType;
            Argument = argument;
            this.IsDistinct = isDistinct;
        }
        public AggregateType AggregateType { get; }
        public Expression Argument { get; }
        public bool IsDistinct { get; }
    }
}

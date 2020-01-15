using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbAggregateExpression
        : DbExpression
    {
        protected internal DbAggregateExpression(Type type, AggregateType aggType, Expression argument, bool isDistinct)
            : base(DbExpressionType.Aggregate, type)
        {
            AggregateType = aggType;
            Argument = argument;
            IsDistinct = isDistinct;
        }
        public AggregateType AggregateType { get; }
        public Expression Argument { get; }
        public bool IsDistinct { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitAggregate(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbAggregate(Type type, AggregateType aggType, Expression argument, bool isDistinct = false)
        {
            return new DbAggregateExpression(type, aggType, argument, isDistinct);
        }
    }
}

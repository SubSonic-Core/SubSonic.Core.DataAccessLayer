using SubSonic.Linq.Expressions.Alias;
using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbAggregateSubQueryExpression
        : DbExpression
    {
        protected internal DbAggregateSubQueryExpression(TableAlias groupByAlias, Expression aggregateInGroupSelect, DbScalarExpression aggregateAsSubquery)
            : base(DbExpressionType.AggregateSubQuery, aggregateAsSubquery.IsNullThrowArgumentNull(nameof(aggregateAsSubquery)).Type)
        {
            AggregateInGroupSelect = aggregateInGroupSelect ?? throw new ArgumentNullException(nameof(aggregateInGroupSelect));
            GroupByAlias = groupByAlias ?? throw new ArgumentNullException(nameof(groupByAlias));
            AggregateAsSubQuery = aggregateAsSubquery ?? throw new ArgumentNullException(nameof(aggregateAsSubquery));
        }
        public TableAlias GroupByAlias { get; }
        public Expression AggregateInGroupSelect { get; }
        public DbScalarExpression AggregateAsSubQuery { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitAggregateSubQuery(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbAggregateSubQuery(TableAlias groupByAlias, Expression aggregateInGroupSelect, DbScalarExpression aggregateAsSubquery)
        {
            return new DbAggregateSubQueryExpression(groupByAlias, aggregateInGroupSelect, aggregateAsSubquery);
        }
    }
}

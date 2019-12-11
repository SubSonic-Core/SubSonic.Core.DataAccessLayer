using SubSonic.Linq.Expressions.Alias;
using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbAggregateSubQueryExpression
        : DbExpression
    {
        public DbAggregateSubQueryExpression(Table groupByAlias, Expression aggregateInGroupSelect, DbScalarExpression aggregateAsSubquery)
            : base(DbExpressionType.AggregateSubQuery, aggregateAsSubquery.IsNullThrowArgumentNull(nameof(aggregateAsSubquery)).Type)
        {
            AggregateInGroupSelect = aggregateInGroupSelect ?? throw new ArgumentNullException(nameof(aggregateInGroupSelect));
            GroupByAlias = groupByAlias ?? throw new ArgumentNullException(nameof(groupByAlias));
            AggregateAsSubQuery = aggregateAsSubquery ?? throw new ArgumentNullException(nameof(aggregateAsSubquery));
        }
        public Table GroupByAlias { get; }
        public Expression AggregateInGroupSelect { get; }
        public DbScalarExpression AggregateAsSubQuery { get; }
    }
}

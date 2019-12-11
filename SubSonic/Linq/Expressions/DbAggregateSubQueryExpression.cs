using SubSonic.Linq.Expressions.Alias;
using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbAggregateSubqueryExpression
        : DbExpression
    {
        public DbAggregateSubqueryExpression(Table groupByAlias, Expression aggregateInGroupSelect, DbScalarExpression aggregateAsSubquery)
            : base(DbExpressionType.AggregateSubquery, aggregateAsSubquery.IsNullThrowMissingArgument(nameof(aggregateAsSubquery)).Type)
        {
            AggregateInGroupSelect = aggregateInGroupSelect ?? throw new ArgumentNullException(nameof(aggregateInGroupSelect));
            GroupByAlias = groupByAlias ?? throw new ArgumentNullException(nameof(groupByAlias));
            AggregateAsSubquery = aggregateAsSubquery ?? throw new ArgumentNullException(nameof(aggregateAsSubquery));
        }
        public Table GroupByAlias { get; }
        public Expression AggregateInGroupSelect { get; }
        public DbScalarExpression AggregateAsSubquery { get; }
    }
}

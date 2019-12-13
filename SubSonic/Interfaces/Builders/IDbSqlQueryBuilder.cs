using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    public interface IDbSqlQueryBuilder
        : IQueryProvider
    {
        Expression BuildComparisonExpression(Expression body, string property, object value, ComparisonOperator @operator, GroupOperator group);
        Expression CallExpression(Expression call, Expression body, ExpressionCallType callType, params string[] properties);
        object ToQueryObject();
    }
}

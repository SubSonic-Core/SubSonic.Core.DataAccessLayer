using SubSonic.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    public interface IDbSqlQueryBuilder
        : IQueryProvider
    {
        Expression BuildComparisonExpression(Expression body, string name, object value, ComparisonOperator op, GroupOperator group);
        Expression BuildSelect(IEnumerable<DbColumnDeclaration> columns, Expression where, IReadOnlyCollection<SubSonicParameter> parameters = null);
        Expression CallExpression(Expression call, Expression body, ExpressionCallType callType, params string[] properties);
        object ToQueryObject();
    }
}

using SubSonic.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    public interface IDbSqlQueryBuilder
        : IQueryProvider
    {
        Expression BuildPredicate(Expression body, DbExpressionType type, string name, object value, ComparisonOperator op, GroupOperator group);
        Expression BuildSelect(IEnumerable<DbColumnDeclaration> columns = null, Expression where = null);
        Expression BuildWhere(Type type, Expression predicate);
        Expression CallExpression(Expression caller, Expression body, ExpressionCallType callType, params string[] properties);
        IDbQueryObject ToQueryObject(Expression expr);
    }
}

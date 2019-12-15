using SubSonic.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    public interface IDbSqlQueryBuilderProvider
        : IQueryProvider
    {
        Expression BuildLogicalBinary(Expression eBody, DbExpressionType type, string name, object value, ComparisonOperator op, GroupOperator group);
        Expression BuildWherePredicate(Expression collection, Expression logical);
        Expression BuildSelect();
        Expression BuildSelect(Expression eWhere);
        Expression BuildSelect(Expression eSelect, Expression eWhere);
        Expression BuildSelect(Expression eSelect, DbExpressionType eType, IEnumerable<Expression> expressions);
        Expression BuildSelect<TEntity, TColumn>(Expression eSelect, Expression<Func<TEntity, TColumn>> selector);
        Expression BuildWhere(DbTableExpression table, Type type, LambdaExpression predicate);
        Expression BuildWhere(Type type, Expression predicate);
        Expression BuildLambda(Expression body, CallType callType, params string[] properties);
        Expression BuildCall(Expression caller, Expression body, CallType callType, params string[] properties);
        IDbQueryObject ToQueryObject(Expression expr);
    }
}

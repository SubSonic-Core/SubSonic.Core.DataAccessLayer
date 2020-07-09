using SubSonic.Infrastructure.Schema;
using SubSonic.Interfaces;
using SubSonic.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Infrastructure
{
    public interface ISubSonicQueryProvider
        : IQueryProvider, IAsyncSubSonicQueryProvider
    {
        DbTableExpression DbTable { get; }


        Expression BuildLogicalIn(Expression body, PropertyInfo property, IQueryable queryable, DbGroupOperator @group);
        Expression BuildLogicalIn(Expression body, PropertyInfo property, IEnumerable<Expression> values, DbGroupOperator @group);
        Expression BuildLogicalBinary(Expression eBody, string name, object value, DbComparisonOperator op, DbGroupOperator group);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="keyData"></param>
        /// <param name="keyNames"></param>
        /// <returns></returns>
        Expression BuildWhereFindByIDPredicate(DbExpression expression, object[] keyData, params string[] keyNames);
        Expression BuildSelect(Expression eSelect, Expression eWhere);
        Expression BuildSelect(Expression eSelect, bool isDistinct);
        Expression BuildSelect(Expression eSelect, int pageNumber, int pageSize);
        Expression BuildSelect(Expression eSelect, IDbEntityProperty properties);
        Expression BuildJoin(JoinType type, Expression left, Expression right);
        Expression BuildLambda(Expression body, LambdaType callType, params string[] properties);

        IDbQuery BuildDbQuery<TEntity>(DbQueryType queryType, IEnumerable<IEntityProxy> proxies);
        IDbQuery ToQuery(Expression expression);
        IDbPagedQuery ToPagedQuery(Expression expression, int size = 20);
    }
}

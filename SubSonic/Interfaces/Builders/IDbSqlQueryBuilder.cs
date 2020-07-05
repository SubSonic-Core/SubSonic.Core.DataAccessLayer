﻿using SubSonic.Infrastructure.Schema;
using SubSonic.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Infrastructure
{
    public interface ISubSonicQueryProvider
        : IQueryProvider
    {
        DbTableExpression DbTable { get; }


        Expression BuildLogicalIn(Expression body, PropertyInfo property, IQueryable queryable, DbGroupOperator @group);
        Expression BuildLogicalIn(Expression body, PropertyInfo property, IEnumerable<Expression> values, DbGroupOperator @group);
        Expression BuildLogicalBinary(Expression eBody, string name, object value, DbComparisonOperator op, DbGroupOperator group);
        Expression BuildWherePredicate(Expression collection, Expression logical);
        Expression BuildWhereFindByIDPredicate(DbTableExpression from, object[] keyData, params string[] keyNames);
        Expression BuildSelect(IQueryable queryable);
        Expression BuildSelect(IQueryable queryable, Expression eWhere);
        Expression BuildSelect(Expression eSelect, Expression eWhere);
        Expression BuildSelect(Expression eSelect, bool isDistinct);
        Expression BuildSelect(Expression eSelect, int count);
        Expression BuildSelect(Expression eSelect, int pageNumber, int pageSize);
        Expression BuildSelect(Expression eSelect, IDbEntityProperty properties);
        Expression BuildSelect(Expression eSelect, IEnumerable<DbOrderByDeclaration> orderBy);
        Expression BuildSelect(Expression eSelect, IEnumerable<Expression> groupBy);
        Expression BuildSelect(Expression eSelect, DbExpressionType eType, IEnumerable<Expression> expressions);
        Expression BuildWhere(DbTableExpression table, Expression where, Type type, LambdaExpression predicate);
        Expression BuildWhereExists<TEntity>(DbTableExpression dbTableExpression, Type type, Expression<Func<TEntity, IQueryable>> query);
        Expression BuildWhereNotExists<TEntity>(DbTableExpression from, Type type, Expression<Func<TEntity, IQueryable>> query);
        Expression BuildJoin(JoinType type, Expression left, Expression right);
        Expression BuildLambda(Expression body, LambdaType callType, params string[] properties);
        Expression BuildCall(string nameOfCallee, Expression collection, Expression lambda);

        IDbQuery BuildDbQuery<TEntity>(DbQueryType queryType, IEnumerable<IEntityProxy> proxies);
        IDbQuery ToQuery(Expression expression);
        IDbPagedQuery ToPagedQuery(Expression expression, int size = 20);
    }
}

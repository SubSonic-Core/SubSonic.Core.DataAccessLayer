using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using Infrastructure.Schema;
    using SysLinq = System.Linq;

    public partial class DbSqlQueryBuilder
    {
        private ParameterExpression _parameter;

        #region properties
        protected ParameterExpression Parameter => _parameter ?? (_parameter = Expression.Parameter(DbEntity.EntityModelType, DbEntity.Name));
        #endregion

        #region Build Select
        public Expression BuildSelect(System.Linq.IQueryable queryable)
        {
            if (queryable.IsNull())
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            return DbExpression.DbSelect(queryable, queryable.Expression.Type, DbTable);
        }

        public Expression BuildSelect(System.Linq.IQueryable queryable, Expression where)
        {
            if (queryable.IsNull())
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            return new DbSelectExpression(queryable, queryable.Expression.Type, DbTable, DbTable.Columns, where);
        }

        public Expression BuildSelect(Expression select, Expression where)
        {
            if (select is DbSelectExpression _select)
            {
                return new DbSelectExpression(_select.QueryObject, _select.Type, _select.From, _select.Columns, where ?? _select.Where, _select.OrderBy, _select.GroupBy, _select.IsDistinct, _select.Take, _select.Skip);
            }

            throw new NotSupportedException();
        }

        public Expression BuildSelect(Expression expression, bool isDistinct)
        {
            if (expression is DbSelectExpression select)
            {
                return new DbSelectExpression(select.QueryObject, select.Type, select.From, select.Columns, select.Where, select.OrderBy, select.GroupBy, isDistinct, select.Take, select.Skip);
            }

            throw new NotSupportedException();
        }

        public Expression BuildSelect(Expression expression, int count)
        {
            if (expression is DbSelectExpression select)
            {
                return new DbSelectExpression(select.QueryObject, select.Type, select.From, select.Columns, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, Expression.Constant(count), select.Skip);
            }

            throw new NotSupportedException();
        }
        public Expression BuildSelect(Expression expression, int pageNumber, int pageSize)
        {
            if (expression is DbSelectExpression select)
            {
                return DbExpression.DbPagedSelect(select, pageNumber, pageSize);
            }
            else if (expression is DbSelectPageExpression paged)
            {
                return DbExpression.DbPagedSelect(paged.Select, pageNumber, pageSize);
            }

            throw new NotSupportedException();
        }

        public Expression BuildSelect(Expression expression, IDbEntityProperty property)
        {
            if (property.IsNull())
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (expression is DbSelectExpression select)
            {
                return new DbSelectExpression(
                    select.QueryObject, 
                    typeof(SysLinq.IQueryable<>).MakeGenericType(property.PropertyType),
                    select.From,
                    select.Columns.Where(x => x.PropertyName.Equals(property.PropertyName, StringComparison.CurrentCulture)), 
                    select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Take, select.Skip);
            }

            throw new NotSupportedException();
        }

        public Expression BuildSelect(Expression expression, IEnumerable<DbOrderByDeclaration> orderBy)
        {
            if (expression is DbSelectExpression select)
            {
                return new DbSelectExpression(select.QueryObject, select.Type, select.From, select.Columns, select.Where, orderBy, select.GroupBy, select.IsDistinct, select.Take, select.Skip);
            }

            throw new NotSupportedException();
        }
        public Expression BuildSelect(Expression expression, IEnumerable<Expression> groupBy)
        {
            if (expression is DbSelectExpression select)
            {
                return new DbSelectExpression(select.QueryObject, select.Type, select.From, select.Columns, select.Where, select.OrderBy, groupBy, select.IsDistinct, select.Take, select.Skip);
            }

            throw new NotSupportedException();
        }

        public Expression BuildSelect(Expression select, DbExpressionType eType, IEnumerable<Expression> expressions)
        {
            if (select is DbSelectExpression)
            {
                throw new NotImplementedException();
            }
            return select;
        }

        private Expression BuildSelect(Expression expression, IEnumerable<DbColumnDeclaration> columns)
        {
            if (expression is DbSelectExpression select)
            {
                return new DbSelectExpression(select.QueryObject, select.Type, select.From, columns, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Take, select.Skip);
            }

            return expression;
        }
        #endregion

        #region Build Where
        public Expression BuildWhere(DbTableExpression table, Expression where, Type type, LambdaExpression predicate)
        {
            if (where.IsNotNull())
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                if (((DbExpressionType)where.NodeType) == DbExpressionType.Where &&
                    where is DbWhereExpression _where)
                {
                    Expression
                        logical = DbWherePredicateBuilder.GetBodyExpression(_where.LambdaPredicate.Body, predicate.Body, DbGroupOperator.AndAlso);
                    predicate = BuildLambda(logical, LambdaType.Predicate) as LambdaExpression;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return DbExpression.DbWhere(table, type, predicate);
        }

        public Expression BuildWhere(DbTableExpression table, Expression where, Type type, Expression predicate)
        {
            LambdaExpression lambda = null;

            if (predicate is UnaryExpression unary)
            {
                if (unary.Operand is LambdaExpression _unary)
                {
                    lambda = _unary;
                }
            }

            return BuildWhere(table, where, type, lambda);
        }

        public Expression BuildWherePredicate(Expression collection, Expression lambda)
        {
            return BuildCall("Where", collection, lambda);
        }

        public Expression BuildWhereFindByIDPredicate(DbTableExpression dbTable, object[] keyData, params string[] keyNames)
        {
            if (keyData.IsNull())
            {
                throw new ArgumentNullException(nameof(keyData));
            }

            Expression
                    logical = null;

            for (int i = 0; i < keyNames.Length; i++)
            {
                logical = BuildLogicalBinary(logical, keyNames[i], keyData[i], DbComparisonOperator.Equal, DbGroupOperator.AndAlso);
            }

            LambdaExpression predicate = (LambdaExpression)BuildLambda(logical, LambdaType.Predicate);

            return BuildWhere(dbTable, null, DbEntity.EntityModelType, predicate);
        }

        public Expression BuildWhereExists<TEntity>(DbTableExpression from, Type type, Expression<Func<TEntity, System.Linq.IQueryable>> select)
        {
            return DbExpression.DbWhere(from, type, select, DbExpressionType.Exists);
        }
        public Expression BuildWhereNotExists<TEntity>(DbTableExpression from, Type type, Expression<Func<TEntity, System.Linq.IQueryable>> select)
        {
            return DbExpression.DbWhere(from, type, select, DbExpressionType.NotExists);
        }
        #endregion

        #region Build Joins
        public Expression BuildJoin(JoinType type, Expression left, Expression right)
        {
            if (left is DbSelectExpression select)
            {
                if (right is DbExpression right_table)
                {
                    return DbExpression.DbSelect(select, (DbJoinExpression)DbExpression.DbJoin(type, select.From, right_table));
                }
            }

            throw new NotSupportedException();
        }
        #endregion

        #region Lambda
        public Expression BuildCall(string nameofCallee, Expression collection, Expression lambda)
        {
            if (lambda.IsNotNull())
            {
                return Expression.Call(
                    typeof(System.Linq.Queryable),
                    nameofCallee,
                    GetTypeArguments((LambdaExpression)lambda),
                    GetMethodCall(collection) ?? Expression.Parameter(GetTypeOf(typeof(ISubSonicCollection<>), DbEntity.EntityModelType)),
                    lambda);
            }
            return lambda;
        }

        public Expression BuildLambda(Expression body, LambdaType @call, params string[] properties)
        {
            Expression result;
            switch (call)
            {
                case Infrastructure.LambdaType.Predicate:
                    {
                        Type fnType = Expression.GetFuncType(Parameter.Type, typeof(bool));

                        result = Expression.Lambda(fnType, body, Parameter);
                    }
                    break;
                case Infrastructure.LambdaType.Selector:
                    {
                        PropertyInfo info = Parameter.Type.GetProperty(properties[0]);
                        Expression property = Expression.Property(Parameter, info);

                        result = Expression.Lambda(Expression.GetFuncType(Parameter.Type, info.PropertyType), property, Parameter);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            return result;
        }

        public Expression BuildLogicalIn(Expression body, PropertyInfo property, SysLinq.IQueryable queryable, DbGroupOperator @group)
        {
            if (property.IsNull())
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (queryable.IsNull())
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            Type constantType = property.PropertyType.GetUnderlyingType();

            MethodInfo method = typeof(SubSonicQueryable).GetGenericMethod(
                nameof(SubSonicQueryable.In),
                new[] { constantType, queryable.Expression.Type });

            Expression
                left = Expression.Property(Parameter, property),
                inside = queryable.Expression,
                right = Expression.Call(null, method, left, inside);

            if (body.IsNull())
            {
                return right;
            }
            else
            {
                return DbWherePredicateBuilder.GetBodyExpression(
                    body,
                    right,
                    @group);
            }
        }

        public Expression BuildLogicalIn(Expression body, PropertyInfo property, IEnumerable<Expression> values, DbGroupOperator @group)
        {
            if (property.IsNull())
            {
                throw new ArgumentNullException(nameof(property));
            }

            Type constantType = property.PropertyType.GetUnderlyingType();

            MethodInfo method = typeof(SubSonicQueryable).GetGenericMethod(
                nameof(SubSonicQueryable.In),
                new[] { constantType, constantType.MakeArrayType() });

            Expression
                left = Expression.Property(Parameter, property),
                inside = Expression.NewArrayInit(constantType, values),
                right = Expression.Call(null, method, left, inside);

            if (body.IsNull())
            {
                return right;
            }
            else
            {
                return DbWherePredicateBuilder.GetBodyExpression(
                    body,
                    right,
                    @group);
            }
        }

        public Expression BuildLogicalBinary(Expression body, string property, object value, DbComparisonOperator @operator, DbGroupOperator @group)
        {
            PropertyInfo propertyInfo = DbEntity.EntityModelType.GetProperty(property);

            Type constantType = propertyInfo.PropertyType.GetUnderlyingType();

            Expression
                left = Expression.Property(Parameter, propertyInfo), 
                right = Expression.Constant(value, constantType); 

            if (body.IsNull())
            {
                return DbWherePredicateBuilder.GetComparisonExpression(left, right, @operator);
            }
            else
            {
                return DbWherePredicateBuilder.GetBodyExpression(body, DbWherePredicateBuilder.GetComparisonExpression(left, right, @operator), @group);
            }
        }
        #endregion

        public IDbQuery ToQuery(Expression expression)
        {
            if (expression.IsNotNull())
            {
                if (expression is DbSelectExpression select)
                {
                    return new DbQuery(
                        select.ToString(),
                        CmdBehavior,
                        GetSubSonicParameters(select));
                }
                else if (expression is DbSelectPageExpression paged)
                {
                    return new DbQuery(
                        paged.ToString(),
                        CmdBehavior,
                        GetSubSonicParameters(paged));
                }
                else if (expression is DbSelectAggregateExpression aggregate)
                {
                    return new DbQuery(
                            aggregate.ToString(),
                            CmdBehavior,
                            GetSubSonicParameters(aggregate)
                        );
                }
                else if (expression is DbInsertExpression insert)
                {
                    return new DbQuery(
                            insert.ToString(),
                            CmdBehavior,
                            GetSubSonicParameters(insert)
                        );
                }
                else if (expression is DbUpdateExpression update)
                {
                    return new DbQuery(
                            update.ToString(),
                            CmdBehavior,
                            GetSubSonicParameters(update)
                        );
                }
                else if (expression is DbDeleteExpression delete)
                {
                    return new DbQuery(
                            delete.ToString(),
                            CmdBehavior,
                            GetSubSonicParameters(delete)
                        );
                }
                else
                {
                    throw new NotSupportedException(expression.GetType().Name);
                }
            }

            return null;
        }

        public IDbPagedQuery ToPagedQuery(Expression expression, int size = 20)
        {
            if (expression is DbSelectExpression select)
            {
                return new DbPagedQuery(select, size);
            }
            else if (expression is DbSelectPageExpression paged)
            {
                return new DbPagedQuery(paged);
            }

            return null;
        }

        private DbParameter[] GetSubSonicParameters(Expression expression)
        {
            if (expression is DbWhereExpression where)
            {
                return where.Parameters.ToArray();
            }
            else if (expression is DbSelectPageExpression paged)
            {
                return paged
                    .Parameters
                    .Union(GetSubSonicParameters(paged.Select.Where))
                    .ToArray();
            }
            else if (expression is DbSelectExpression select)
            {
                return GetSubSonicParameters(select.Where);
            }
            else if(expression is DbSelectAggregateExpression aggregate)
            {
                return GetSubSonicParameters(aggregate.Where);
            }
            else if (expression is DbInsertExpression insert)
            {
                return insert
                    .DbParameters
                    .ToArray();
            }
            else if (expression is DbUpdateExpression update)
            {
                return update
                    .DbParameters
                    .Union(GetSubSonicParameters(update.Where))
                    .ToArray();
            }
            else if (expression is DbDeleteExpression delete)
            {
                return GetSubSonicParameters(delete.Where);
            }

            return Array.Empty<SubSonicParameter>();
        }

        protected virtual Expression BuildQuery(Expression expression)
        {
            if (expression.IsNotNull())
            {
                SqlQueryType = GetQueryType(expression);
            }

            if (SqlQueryType == DbSqlQueryType.Unknown)
            {   /// most likely expression is from the DbExpression base implementation
                if (expression is MethodCallExpression call)
                {
                    if (call.Method.IsOrderBy(out OrderByType orderByType))
                    {
                        return BuildSelectWithOrderByDeclaration(call, orderByType);
                    }
                    else if (call.Method.IsWhere())
                    {
                        return BuildSelectWithWhere(call);
                    }
                    else if (call.Method.IsTake())
                    {
                        return BuildSelectWithTake(call);
                    }
                    else if (call.Method.IsSkip())
                    {
                        return BuildSelectWithSkip(call);
                    }
                    else if (call.Method.IsDistinct())
                    {
                        return BuildSelectWithDistinct(call);
                    }
                    else if (call.Method.IsSupportedSelect())
                    {
                        return BuildSelectWithExpression(call);
                    }
                    else
                    {
                        throw Error.NotSupported(SubSonicErrorMessages.UnSupportedMethodException.Format(call.Method.Name));
                    }
                }
            }

            return expression ?? DbEntity.Table;
        }

        private Type GetTypeOf(Type type, params Type[] types)
        {
            return type.IsGenericType ? type.MakeGenericType(types) : type;
        }

        private static Type[] GetTypeArguments(LambdaExpression expression)
        {
            if (expression.IsNotNull())
            {
                IEnumerable<Type> types = expression.Parameters.Select(Param => Param.Type);

                if (!expression.Body.Type.IsBoolean())
                {   // not a predicate
                    types = types.Union(new[] { expression.Body.Type });
                }

                return types.ToArray();
            }
            return Array.Empty<Type>();
        }

        private Expression BuildSelectWithExpression(MethodCallExpression expression)
        {
            if (!(expression is null))
            {
                DbSelectExpression select = null;
                LambdaExpression selector = null;

                foreach (var argument in expression.Arguments)
                {
                    if (argument is DbSelectExpression _select)
                    {
                        select = _select;
                    }
                    else if (argument is UnaryExpression unary)
                    {
                        if (unary.Operand is LambdaExpression lambda)
                        {
                            selector = lambda;
                        }
                    }
                }

                return BuildSelect(select, select.Columns.Where(column => column.PropertyName.Equals(selector.GetProperty().Name, StringComparison.CurrentCulture)));
            }

            return expression;
        }

        private Expression BuildSelectWithDistinct(MethodCallExpression expression)
        {
            if (!(expression is null))
            {
                DbSelectExpression select = null;

                foreach (var argument in expression.Arguments)
                {
                    if (argument is DbSelectExpression _select)
                    {
                        select = _select;
                    }
                }

                return BuildSelect(select, true);                    
            }

            return expression;
        }

        private Expression BuildSelectWithSkip(MethodCallExpression expression)
        {
            if (!(expression is null))
            {
                DbSelectExpression select = null;
                Expression skip = null;

                foreach (var argument in expression.Arguments)
                {
                    if (argument is DbSelectExpression _select)
                    {
                        select = _select;
                    }
                    else if (argument is ConstantExpression constant)
                    {
                        skip = constant;
                    }
                }

                return DbExpression.DbSelect(
                        select,
                        select.Take,
                        skip
                        );
            }

            return expression;
        }

        private Expression BuildSelectWithTake(MethodCallExpression expression)
        {
            if (!(expression is null))
            {
                DbSelectExpression select = null;
                Expression take = null;

                foreach (var argument in expression.Arguments)
                {
                    if (argument is DbSelectExpression _select)
                    {
                        select = _select;
                    }
                    else if (argument is ConstantExpression constant)
                    {
                        take = constant;
                    }
                }

                return DbExpression.DbSelect(
                        select,
                        take,
                        select.Skip
                        );
            }

            return expression;
        }

        private Expression BuildSelectWithWhere(MethodCallExpression expression)
        {
            if (!(expression is null))
            {
                DbSelectExpression select = null;
                LambdaExpression where = null;

                foreach (var argument in expression.Arguments)
                {
                    if (argument is DbSelectExpression _select)
                    {
                        select = _select;
                    }
                    else if (argument is UnaryExpression unary)
                    {
                        if (unary.Operand is LambdaExpression _unary)
                        {
                            where = _unary;
                        }                        
                    }
                }

                return DbExpression.DbSelect(
                        select,
                        DbExpression.DbWhere(select.From, select.Type, where));
            }

            return expression;
        }

        private Expression BuildSelectWithOrderByDeclaration(MethodCallExpression expression, OrderByType orderByType)
        {
            DbSelectExpression select = null;
            UnaryExpression unary = null;
            List<DbOrderByDeclaration> orderBy = new List<DbOrderByDeclaration>();

            bool clearOrderBy = expression.Method.Name.In(nameof(Queryable.OrderBy), nameof(Queryable.OrderByDescending));

            foreach (var argument in expression.Arguments)
            {
                if (argument is DbSelectExpression _select)
                {
                    select = _select;
                }
                else if (argument is UnaryExpression _unary)
                {
                    unary = _unary;
                }
            }

            if (!clearOrderBy)
            {
                orderBy.AddRange(select.OrderBy);
            }

            if (unary.Operand is LambdaExpression lamda)
            {
                if (lamda.Body is MemberExpression member)
                {
                    orderBy.Add(new DbOrderByDeclaration(orderByType, select.From.Columns.Single(column => column.PropertyName == member.Member.Name).Expression));
                }
            }
            return DbExpression.DbSelect(select.QueryObject, select.Type, select.From, select.Columns, select.Where, orderBy);
        }
    }
}

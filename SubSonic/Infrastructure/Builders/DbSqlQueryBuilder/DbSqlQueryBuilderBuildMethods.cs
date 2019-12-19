using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using System.Reflection;

    public partial class DbSqlQueryBuilder
    {
        #region properties
        protected ParameterExpression Parameter => Expression.Parameter(DbEntity.EntityModelType, DbEntity.Name);
        #endregion

        #region Build Select
        public Expression BuildSelect()
        {
            return new DbSelectExpression(DbTable.Alias, DbTable.Columns, DbTable);
        }

        public Expression BuildSelect(Expression where)
        {
            return new DbSelectExpression(DbTable.Alias, DbTable.Columns, DbTable, where);
        }

        public Expression BuildSelect(Expression select, Expression where)
        {
            if (select is DbSelectExpression)
            {
                DbSelectExpression _select = (DbSelectExpression)select;

                return new DbSelectExpression(_select.Alias, _select.Columns, _select.From, where);
            }
            return BuildSelect(where);
        }

        public Expression BuildSelect(Expression select, DbExpressionType eType, IEnumerable<Expression> expressions)
        {
            if (select is DbSelectExpression)
            {
                throw new NotImplementedException();
            }
            return select;
        }

        public Expression BuildSelect<TEntity, TColumn>(Expression expression, Expression<Func<TEntity, TColumn>> selector)
        {
            if (expression is DbSelectExpression)
            {
                DbSelectExpression _select = (DbSelectExpression)expression;

                return new DbSelectExpression(_select.Alias, _select.Columns.Where(col => col.PropertyName == selector.GetPropertyName()), _select.From, _select.Where, _select.OrderBy, _select.GroupBy, _select.IsDistinct, _select.Skip, _select.Take);
            }
            else if (expression is DbTableExpression)
            {
                DbTableExpression table = expression as DbTableExpression;

                return new DbSelectExpression(table.Alias, table.Columns.Where(col => col.PropertyName == selector.GetPropertyName()), table);
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

                if (where is DbWhereExpression)
                {
                    Expression
                        logical = DbWherePredicateBuilder.GetBodyExpression(((DbWhereExpression)where).LambdaPredicate.Body, predicate.Body, GroupOperator.AndAlso);
                    predicate = BuildLambda(logical, LambdaType.Predicate) as LambdaExpression;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return DbExpression.Where(table, type, predicate);
        }

        public Expression BuildWherePredicate(Expression collection, Expression lambda)
        {
            return BuildCall("Where", collection, lambda);
        }

        public Expression BuildWhereExists<TEntity>(DbTableExpression dbTableExpression, object p, Type type, Expression<System.Linq.IQueryable<TEntity>> select)
        {
            throw new NotImplementedException();
        }
        public Expression BuildWhereNotExists<TEntity>(DbTableExpression from, Expression where, Type type, Expression<System.Linq.IQueryable<TEntity>> select)
        {
            throw new NotImplementedException();
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
            Expression result = null;
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

        public Expression BuildLogicalBinary(Expression body, DbExpressionType type, string property, object value, ComparisonOperator @operator, GroupOperator @group)
        {
            ParameterExpression parameter = Expression.Parameter(DbEntity.EntityModelType, DbEntity.QualifiedName);
            PropertyInfo propertyInfo = DbEntity.EntityModelType.GetProperty(property);

            Type constantType = propertyInfo.PropertyType.GetUnderlyingType();

            Expression
                left = Expression.Property(parameter, propertyInfo), // GetDbColumnExpression(propertyInfo),
                right = Expression.Constant(value, constantType); // GetNamedExpression(type, propertyInfo, value);

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

        public IDbQueryObject ToQueryObject(Expression exp)
        {
            if (exp is DbSelectExpression)
            {
                DbSelectExpression select = exp as DbSelectExpression;

                return new DbQueryObject(select.ToString(), ((DbWhereExpression)select.Where)?.Parameters);
            }
            return null;
        }

        protected virtual Expression BuildQuery(Expression expression)
        {
            if (expression.IsNotNull())
            {
                SqlQueryType = GetQueryType(expression);
            }

            return expression ?? DbEntity.Expression;
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
    }
}

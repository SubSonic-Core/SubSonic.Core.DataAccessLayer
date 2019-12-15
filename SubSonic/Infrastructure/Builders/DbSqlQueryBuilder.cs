using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure.Builders
{
    using Linq.Expressions;
    using Logging;
    using Schema;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    public class DbSqlQueryBuilder<TEntity>
        : DbSqlQueryBuilder
        , ISubSonicQueryProvider<TEntity>
    {
        public DbSqlQueryBuilder(ISubSonicLogger<TEntity> logger)
            : base(typeof(TEntity), logger)
        {
        }

        public DbAliasedExpression GetAliasedTable() => DbTable;
    }

    public class DbSqlQueryBuilder
        : DbExpressionAccessor
        , IDbSqlQueryBuilderProvider
    {
        private readonly ISubSonicLogger logger;
        private readonly SubSonicParameterDictionary parameters;

        public DbSqlQueryBuilder(Type dbModelType, ISubSonicLogger logger = null)
        {
            if (dbModelType is null)
            {
                throw new ArgumentNullException(nameof(dbModelType));
            }

            this.logger = logger ?? DbContext.ServiceProvider.GetService<ISubSonicLogger>();
            DbEntity = DbContext.DbModel.GetEntityModel(dbModelType.GetQualifiedType());
            DbTable = DbEntity.Expression;
            parameters = new SubSonicParameterDictionary();
        }

        public SqlQueryType SqlQueryType { get; private set; }

        public IDbEntityModel DbEntity { get; }
        public DbTableExpression DbTable { get; }

        protected ParameterExpression Parameter => Expression.Parameter(DbEntity.EntityModelType, DbEntity.Name);

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

        public Expression BuildWhere(DbTableExpression table, Type type, LambdaExpression predicate)
        {
            return DbExpression.Where(table, type, predicate);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            using (IPerformanceLogger performance = logger.Start(GetType(), nameof(CreateQuery)))
            {
                return new SubSonicCollection(DbEntity.EntityModelType, this, BuildQuery(expression));
            }
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
        {
            return new SubSonicCollection<TEntity>(this, BuildQuery(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        public object Execute(Expression expression)
        {
            using (AutomaticConnectionScope Scope = DbContext.ServiceProvider.GetService<AutomaticConnectionScope>())
            using (var perf = logger.Start(GetType(), nameof(Execute)))
            {
                throw new NotImplementedException();
            }
        }

        protected virtual Expression BuildQuery(Expression expression)
        {
            if (expression.IsNotNull())
            {
                SqlQueryType = GetQueryType(expression);
            }

            return expression ?? DbEntity.Expression;
        }

        protected virtual SqlQueryType GetQueryType(Expression expression)
        {
            if (expression.IsNotNull())
            {
                if (!expression.NodeType.IsDbExpression())
                {
                    throw new NotImplementedException();
                }

                switch((DbExpressionType)expression.NodeType)
                {
                    case DbExpressionType.Table:
                        return SqlQueryType.Unknown;
                    case DbExpressionType.Select:
                        return SqlQueryType.Read;
                    default:
                        throw new NotSupportedException();
                }
            }

            throw new ArgumentNullException(nameof(expression));
        }



        public Expression BuildCall(string nameofCallee, Expression collection, Expression lambda)
        {
            if (lambda.IsNotNull())
            {
                return Expression.Call(
                    typeof(Queryable),
                    nameofCallee,
                    GetTypeArguments((LambdaExpression)lambda),
                    GetMethodCall(collection) ?? Expression.Parameter(GetTypeOf(typeof(ISubSonicCollection<>), DbEntity.EntityModelType)),
                    lambda);
            }
            return lambda;
        }

        private Type GetTypeOf(Type type, params Type[] types)
        {
            return type.IsGenericType ? type.MakeGenericType(types) : type;
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

        public Expression BuildWherePredicate(Expression collection, Expression lambda)
        {
            return BuildCall("Where", collection, lambda);
        }
        
        public IDbQueryObject ToQueryObject(Expression exp)
        {
            if (exp is DbSelectExpression)
            {
                DbSelectExpression select = exp as DbSelectExpression;

                return new DbQueryObject(select.ToString(), ((DbWhereExpression)select.Where)?.Parameters);
            }
            return null;
        }
    }
}

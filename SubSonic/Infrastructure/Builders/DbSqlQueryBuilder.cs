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
    }

    public class DbSqlQueryBuilder
        : DbExpressionAccessor
        , IDbSqlQueryBuilder
    {
        private readonly ISubSonicLogger logger;
        private readonly SubSonicParameterCollection parameters;

        public DbSqlQueryBuilder(Type dbModelType, ISubSonicLogger logger = null)
        {
            if (dbModelType is null)
            {
                throw new ArgumentNullException(nameof(dbModelType));
            }

            this.logger = logger ?? DbContext.ServiceProvider.GetService<ISubSonicLogger>();
            DbEntity = DbContext.DbModel.GetEntityModel(dbModelType.GetQualifiedType());
            DbTable = DbEntity.Expression;
            parameters = new SubSonicParameterCollection();
        }

        public SqlQueryType SqlQueryType { get; private set; }

        public IDbEntityModel DbEntity { get; }
        public DbTableExpression DbTable { get; }

        protected ParameterExpression Parameter => Expression.Parameter(DbEntity.EntityModelType, DbEntity.Name);

        public ISqlQueryProvider SqlQueryProvider { get; private set; }

        private void CheckBuilderState()
        {
            if (SqlQueryProvider.IsNull())
            {
                throw new InvalidOperationException();
            }
        }

        public Expression BuildSelect(IEnumerable<DbColumnDeclaration> columns = null, Expression where = null, IReadOnlyCollection<SubSonicParameter> parameters = null)
        {
            return new DbSelectExpression(DbTable.Alias, columns ?? DbTable.Columns, DbTable, where, parameters ?? this.parameters.ToReadOnly());
        }

        public IDbSqlQueryBuilder BuildSqlQuery(SqlQueryType sqlQueryType, ISqlQueryProvider sqlQueryProvider)
        {
            SqlQueryType = sqlQueryType;
            SqlQueryProvider = sqlQueryProvider ?? throw new ArgumentNullException(nameof(sqlQueryProvider));

            return this;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            using (IPerformanceLogger performance = logger.Start(GetType(), nameof(CreateQuery)))
            {
                return new SubSonicCollection(DbEntity.EntityModelType, BuildQuery(expression));
            }
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
        {
            return new SubSonicCollection<TEntity>(BuildQuery(expression));
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
                    case DbExpressionType.Select:
                        return SqlQueryType.Read;
                    default:
                        throw new NotSupportedException();
                }
            }

            throw new ArgumentNullException(nameof(expression));
        }

        public Expression CallExpression(Expression source, Expression body, ExpressionCallType callType , params string[] properties)
        {
            if (body.IsNotNull())
            {
                Expression lambda = GetExpressionArgument(body, callType, properties);

                return Expression.Call(
                    typeof(Queryable),
                    callType.ToString(),
                    GetTypeArguments(callType, lambda),
                    GetMethodCall(source) ?? Expression.Parameter(GetTypeOf(typeof(ISubSonicCollection<>), DbEntity.EntityModelType)),
                    lambda);
            }
            return body;
        }

        private Type GetTypeOf(Type type, params Type[] types)
        {
            return type.IsGenericType ? type.MakeGenericType(types) : type;
        }

        private Expression GetExpressionArgument(Expression body, ExpressionCallType @call, params string[] properties)
        {
            Expression result = null;
            switch (call)
            {
                case Infrastructure.ExpressionCallType.Where:
                    {
                        Type fnType = Expression.GetFuncType(Parameter.Type, typeof(bool));

                        result = Expression.Lambda(fnType, body, Parameter);
                    }
                    break;
                case Infrastructure.ExpressionCallType.OrderBy:
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

        private static Type[] GetTypeArguments(ExpressionCallType @enum, Expression expression)
        {
            IEnumerable<Type> types = Array.Empty<Type>();

            switch (@enum)
            {
                case Infrastructure.ExpressionCallType.Where:
                    {
                        types = GetParameterTypes((LambdaExpression)expression);
                    }
                    break;
                case Infrastructure.ExpressionCallType.OrderBy:
                    {
                        types = GetParameterTypes((LambdaExpression)expression)
                            .Union(GetMemberType((LambdaExpression)expression));
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return types.ToArray();
        }

        private static IEnumerable<Type> GetParameterTypes(LambdaExpression expression) => expression.Parameters.Select(Param => Param.Type);

        private static IEnumerable<Type> GetMemberType(LambdaExpression expression) => new[] { expression.Body.Type };

        public Expression BuildComparisonExpression(Expression body, string property, object value, ComparisonOperator @operator, GroupOperator @group)
        {
            PropertyInfo propertyInfo = DbEntity.EntityModelType.GetProperty(property);

            Expression
                left = GetDbColumnExpression(propertyInfo),
                right = GetNamedExpression(propertyInfo, value);

            if (body.IsNull())
            {
                return GetComparisonExpression(left, right, @operator);
            }
            else
            {
                return GetBodyExpression(body, GetComparisonExpression(left, right, @operator), @group);
            }
        }

        private Expression GetNamedExpression(PropertyInfo info, object value)
        {
            IDbEntityProperty property = DbEntity[info.Name];

            parameters.Add(new SubSonicParameter(property, $"@{property.Name}") { Value = value });

            Type ConstantType = info.PropertyType.GetUnderlyingType();

            return new DbNamedValueExpression(
                property.Name,
                Expression.Constant(Convert.ChangeType(value, ConstantType, CultureInfo.CurrentCulture), ConstantType));
        }

        private Expression GetDbColumnExpression(PropertyInfo info)
        {
            foreach(DbColumnDeclaration column in DbTable.Columns)
            {
                if(column.PropertyName == info.Name)
                {
                    return column.Expression;
                }
            }
            return null;
        }

        private Expression GetBodyExpression(Expression body, Expression right, GroupOperator @group)
        {
            Expression result;

            switch (group)
            {
                case GroupOperator.And:
                    {
                        result = Expression.And(body, right);
                    }
                    break;
                case GroupOperator.AndAlso:
                    {
                        result = Expression.AndAlso(body, right);
                    }
                    break;
                case GroupOperator.Or:
                    {
                        result = Expression.Or(body, right);
                    }
                    break;
                case GroupOperator.OrElse:
                    {
                        result = Expression.OrElse(body, right);
                    }
                    break;
                default:
                    throw new NotImplementedException($"{@group} operation is not implemented.");
            }

            return result;
        }

        private static Expression GetComparisonExpression(Expression left, Expression right, ComparisonOperator @operator)
        {
            Expression result;

            switch (@operator)
            {
                case ComparisonOperator.Contains:
                case ComparisonOperator.NotContains:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("Contains", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == ComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case ComparisonOperator.StartsWith:
                case ComparisonOperator.NotStartsWith:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("StartsWith", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == ComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case ComparisonOperator.EndsWith:
                case ComparisonOperator.NotEndsWith:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("EndsWith", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == ComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case ComparisonOperator.Equal:
                    {
                        result = Expression.Equal(left, right);
                    }
                    break;
                case ComparisonOperator.NotEqual:
                    {
                        result = Expression.NotEqual(left, right);
                    }
                    break;
                case ComparisonOperator.GreaterThan:
                    {
                        result = Expression.GreaterThan(left, right);
                    }
                    break;
                case ComparisonOperator.GreaterThanOrEqual:
                    {
                        result = Expression.GreaterThanOrEqual(left, right);
                    }
                    break;
                case ComparisonOperator.LessThan:
                    {
                        result = Expression.LessThan(left, right);
                    }
                    break;
                case ComparisonOperator.LessThanOrEqual:
                    {
                        result = Expression.LessThanOrEqual(left, right);
                    }
                    break;
                default:
                    throw new NotImplementedException($"{@operator} operation is not implemented.");
            }

            return result;
        }

        public IDbQueryObject ToQueryObject(Expression exp)
        {
            if (exp is DbSelectExpression)
            {
                DbSelectExpression select = exp as DbSelectExpression;

                return new DbQueryObject(select.ToString(), select.Parameters);
            }
            return null;
        }

        
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace SubSonic
{
    using Linq;

    public class DbExpressionBuilder
    {
        private readonly ParameterExpression parameter;
        private readonly ConstantExpression root;
        //private MethodCallExpression call;
        private Expression body;

        public DbExpressionBuilder(
            ParameterExpression parameter,
            ConstantExpression root)
        {
            this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            this.root = root ?? throw new ArgumentNullException(nameof(root));
        }

        //public Expression ToMethodCallExpression() => call;

        public DbExpressionBuilder BuildComparisonExpression(string property, object value, DbComparisonOperator @operator, DbGroupOperator @group)
        {
            PropertyInfo propertyInfo = parameter.Type.GetProperty(property);

            Type ConstantType = propertyInfo.PropertyType.GetUnderlyingType();

            Expression
                left = Expression.Property(parameter, propertyInfo),
                right = Expression.Constant(Convert.ChangeType(value, ConstantType, CultureInfo.CurrentCulture), ConstantType);

            if(body is null)
            {
                body = GetComparisonExpression(left, right, @operator);
            }
            else
            {
                body = GetBodyExpression(GetComparisonExpression(left, right, @operator), @group);
            }

            return this;
        }

        //public DbExpressionBuilder ForEachProperty(string[] properties, Action<string> action)
        //{
        //    foreach(string property in properties)
        //    {
        //        action(property);
        //    }

        //    return this;
        //}
            

        //public DbExpressionBuilder CallExpression<TEntity>(LambdaType @enum, params string[] properties)
        //{
        //    Expression lambda = GetExpressionArgument<TEntity>(@enum, properties);

        //    this.call = Expression.Call(
        //        typeof(System.Linq.Queryable),
        //        @enum.ToString(),
        //        GetTypeArguments(@enum, lambda),
        //        (Expression)call ?? root,
        //        lambda);

        //    return this;
        //}

        private static Type[] GetTypeArguments(LambdaType @enum, Expression expression)
        {
            IEnumerable<Type> types = Array.Empty<Type>();
            
            switch(@enum)
            {
                case LambdaType.Predicate:
                    {
                        types = GetParameterTypes((LambdaExpression)expression);
                    }
                    break;
                case LambdaType.Selector:
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

        private Expression GetExpressionArgument<TEntity>(LambdaType @call, params string[] properties)
        {
            Expression result;
            switch(call)
            {
                case LambdaType.Predicate:
                    {
                        result = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
                    }
                    break;
                case LambdaType.Selector:
                    {
                        PropertyInfo info = typeof(TEntity).GetProperty(properties[0]);
                        Expression property = Expression.Property(parameter, info);
                         
                        result = Expression.Lambda(Expression.GetFuncType(parameter.Type, info.PropertyType), property, parameter);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            return result;
        }

        private Expression GetBodyExpression(Expression right, DbGroupOperator @group)
        {
            Expression result;

            switch(group)
            {
                case DbGroupOperator.And:
                    {
                        result = Expression.And(body, right);
                    }
                    break;
                case DbGroupOperator.AndAlso:
                    {
                        result = Expression.AndAlso(body, right);
                    }
                    break;
                case DbGroupOperator.Or:
                    {
                        result = Expression.Or(body, right);
                    }
                    break;
                case DbGroupOperator.OrElse:
                    {
                        result = Expression.OrElse(body, right);
                    }
                    break;
                default:
                    throw new NotImplementedException($"{@group} operation is not implemented.");
            }

            return result;
        }

        private static Expression GetComparisonExpression(Expression left, Expression right, DbComparisonOperator @operator)
        {
            Expression result;

            switch(@operator)
            {
                case DbComparisonOperator.Contains:
                case DbComparisonOperator.NotContains:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("Contains", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == DbComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case DbComparisonOperator.StartsWith:
                case DbComparisonOperator.NotStartsWith:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("StartsWith", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == DbComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case DbComparisonOperator.EndsWith:
                case DbComparisonOperator.NotEndsWith:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("EndsWith", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == DbComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case DbComparisonOperator.Equal:
                    {
                        result = Expression.Equal(left, right);
                    }
                    break;
                case DbComparisonOperator.NotEqual:
                    {
                        result = Expression.NotEqual(left, right);
                    }
                    break;
                case DbComparisonOperator.GreaterThan:
                    {
                        result = Expression.GreaterThan(left, right);
                    }
                    break;
                case DbComparisonOperator.GreaterThanOrEqual:
                    {
                        result = Expression.GreaterThanOrEqual(left, right);
                    }
                    break;
                case DbComparisonOperator.LessThan:
                    {
                        result = Expression.LessThan(left, right);
                    }
                    break;
                case DbComparisonOperator.LessThanOrEqual:
                    {
                        result = Expression.LessThanOrEqual(left, right);
                    }
                    break;
                default:
                    throw new NotImplementedException($"{@operator} operation is not implemented.");
            }

            return result;
        }
    }
}

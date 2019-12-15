using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbExpressionBuilder
    {
        private readonly ParameterExpression parameter;
        private readonly ConstantExpression root;
        private MethodCallExpression call;
        private Expression body;

        public DbExpressionBuilder(
            ParameterExpression parameter,
            ConstantExpression root)
        {
            this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            this.root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public Expression ToMethodCallExpression() => call;

        public DbExpressionBuilder BuildComparisonExpression(string property, object value, ComparisonOperator @operator, GroupOperator @group)
        {
            PropertyInfo propertyInfo = parameter.Type.GetProperty(property);

            Type ConstantType = propertyInfo.PropertyType.GetUnderlyingType();

            Expression
                left = Expression.Property(parameter, propertyInfo),
                right = Expression.Constant(Convert.ChangeType(value, ConstantType, CultureInfo.CurrentCulture), ConstantType);

            if(body.IsNull())
            {
                body = GetComparisonExpression(left, right, @operator);
            }
            else
            {
                body = GetBodyExpression(GetComparisonExpression(left, right, @operator), @group);
            }

            return this;
        }

        public DbExpressionBuilder ForEachProperty(string[] properties, Action<string> action)
        {
            foreach(string property in properties)
            {
                action(property);
            }

            return this;
        }
            

        public DbExpressionBuilder CallExpression<TEntity>(LambdaType @enum, params string[] properties)
        {
            Expression lambda = GetExpressionArgument<TEntity>(@enum, properties);

            this.call = Expression.Call(
                typeof(Queryable),
                @enum.ToString(),
                GetTypeArguments(@enum, lambda),
                (Expression)call ?? root,
                lambda);

            return this;
        }

        private static Type[] GetTypeArguments(LambdaType @enum, Expression expression)
        {
            IEnumerable<Type> types = Array.Empty<Type>();
            
            switch(@enum)
            {
                case Infrastructure.LambdaType.Predicate:
                    {
                        types = GetParameterTypes((LambdaExpression)expression);
                    }
                    break;
                case Infrastructure.LambdaType.Selector:
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
                case Infrastructure.LambdaType.Predicate:
                    {
                        result = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
                    }
                    break;
                case Infrastructure.LambdaType.Selector:
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

        private Expression GetBodyExpression(Expression right, GroupOperator @group)
        {
            Expression result;

            switch(group)
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

            switch(@operator)
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
    }
}

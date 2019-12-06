using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbExpressionBuilder
    {
        private readonly ParameterExpression parameter;
        private Expression body;

        public DbExpressionBuilder(ParameterExpression expression)
        {
            parameter = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public ParameterExpression Parameter => parameter;

        public Expression Body => body;

        public DbExpressionBuilder BuildComparisonExpression(string property, object value, EnumComparisonOperator @operator, EnumGroupOperator @group)
        {
            PropertyInfo propertyInfo = parameter.Type.GetProperty(property);

            Type ConstantType = propertyInfo.PropertyType.GetUnderlyingType();

            Expression
                left = Expression.Property(parameter, propertyInfo),
                right = Expression.Constant(Convert.ChangeType(value, ConstantType), ConstantType);

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

        private Expression GetBodyExpression(Expression right, EnumGroupOperator @group)
        {
            Expression result;

            switch(group)
            {
                case EnumGroupOperator.And:
                    {
                        result = Expression.And(body, right);
                    }
                    break;
                case EnumGroupOperator.AndAlso:
                    {
                        result = Expression.AndAlso(body, right);
                    }
                    break;
                case EnumGroupOperator.Or:
                    {
                        result = Expression.Or(body, right);
                    }
                    break;
                case EnumGroupOperator.OrElse:
                    {
                        result = Expression.OrElse(body, right);
                    }
                    break;
                default:
                    throw new NotImplementedException($"{@group} operation is not implemented.");
            }

            return result;
        }

        private Expression GetComparisonExpression(Expression left, Expression right, EnumComparisonOperator @operator)
        {
            Expression result;

            switch(@operator)
            {
                case EnumComparisonOperator.Contains:
                case EnumComparisonOperator.NotContains:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("Contains", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == EnumComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case EnumComparisonOperator.StartsWith:
                case EnumComparisonOperator.NotStartsWith:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("StartsWith", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == EnumComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case EnumComparisonOperator.EndsWith:
                case EnumComparisonOperator.NotEndsWith:
                    {
                        MethodInfo
                            oMethod = left.Type.GetMethod("EndsWith", new Type[] { right.Type });

                        result = Expression.Call(left, oMethod, right);

                        if (@operator == EnumComparisonOperator.NotContains)
                        {
                            result = Expression.Not(result);
                        }
                    }
                    break;
                case EnumComparisonOperator.Equal:
                    {
                        result = Expression.Equal(left, right);
                    }
                    break;
                case EnumComparisonOperator.NotEqual:
                    {
                        result = Expression.NotEqual(left, right);
                    }
                    break;
                case EnumComparisonOperator.GreaterThan:
                    {
                        result = Expression.GreaterThan(left, right);
                    }
                    break;
                case EnumComparisonOperator.GreaterThanOrEqual:
                    {
                        result = Expression.GreaterThanOrEqual(left, right);
                    }
                    break;
                case EnumComparisonOperator.LessThan:
                    {
                        result = Expression.LessThan(left, right);
                    }
                    break;
                case EnumComparisonOperator.LessThanOrEqual:
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

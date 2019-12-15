using SubSonic.Infrastructure.Schema;
using SubSonic.Linq.Expressions;
using SubSonic.Linq.Expressions.Structure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure.Builders
{
    partial class DbWherePredicateBuilder
        : DbExpressionVisitor
    {
        private readonly DbTableExpression table;
        private SubSonicParameterDictionary parameters;
        private Expression body;

        private GroupOperator group;
        private ComparisonOperator comparison;
        private PropertyInfo propertyInfo;
        private Expression left, right;

        protected DbWherePredicateBuilder(DbTableExpression table)
        {
            parameters = new SubSonicParameterDictionary();
            this.table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public static DbExpression GetWherePredicate(DbTableExpression table, Type type, Expression predicate)
        {
            var builder = new DbWherePredicateBuilder(table);

            return new DbWhereExpression(type, builder.ParsePredicate(predicate), builder.parameters.ToReadOnlyCollection(DbExpressionType.Where));
        }

        public static Expression GetComparisonExpression(Expression left, Expression right, ComparisonOperator @operator)
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

        public static Expression GetBodyExpression(Expression body, Expression right, GroupOperator @group)
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

        public Expression ParsePredicate(Expression predicate)
        {
            Visit(predicate);


            return body;
        }

        private Expression GetDbColumnExpression(PropertyInfo info)
        {
            foreach (DbColumnDeclaration column in table.Columns)
            {
                if (column.PropertyName == info.Name)
                {
                    propertyInfo = info;
                    return column.Expression;
                }
            }
            return null;
        }

        private Expression GetNamedExpression(object value)
        {
            IDbEntityProperty property = table.Model[propertyInfo.Name];

            parameters.Add(DbExpressionType.Where, new SubSonicParameter(property, $"@{property.Name}") { Value = value });

            Type ConstantType = propertyInfo.PropertyType.GetUnderlyingType();

            return new DbNamedValueExpression(
                property.Name,
                Expression.Constant(Convert.ChangeType(value, ConstantType, CultureInfo.CurrentCulture), ConstantType));
        }
    }
}

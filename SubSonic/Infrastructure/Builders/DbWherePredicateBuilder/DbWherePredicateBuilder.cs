using SubSonic.Infrastructure.Schema;
using SubSonic.Linq;
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
        [ThreadStatic]
        private static Stack<DbWherePredicateBuilder> __instances;

        private readonly DbTableExpression table;
        private SubSonicParameterDictionary parameters;
        private Expression body;

        private GroupOperator group;
        private ComparisonOperator comparison;
        private DbExpressionType whereType;
        private PropertyInfo propertyInfo;
        private FieldInfo fieldInfo;
        private Expression left, right;

        protected DbWherePredicateBuilder(DbExpressionType whereType, DbTableExpression table)
        {
            parameters = new SubSonicParameterDictionary();
            this.whereType = whereType;
            this.table = table ?? throw new ArgumentNullException(nameof(table));

            if (__instances is null)
            {
                __instances = new Stack<DbWherePredicateBuilder>();
            }

            __instances.Push(this);
        }

        public static DbExpression GetWherePredicate(Type type, LambdaExpression lambda, DbExpressionType whereType, DbTableExpression table)
        {
            using (var builder = new DbWherePredicateBuilder(whereType, table))
            {
                return new DbWhereExpression(whereType, type, lambda, builder.ParseLambda(lambda), builder.parameters.ToReadOnlyCollection(DbExpressionType.Where));
            }
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
                case ComparisonOperator.In:
                    result = DbExpression.DbIn(left, right);
                    break;
                case ComparisonOperator.NotIn:
                    result = DbExpression.DbNotIn(left, right);
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

        public Expression ParseLambda(Expression predicate)
        {
            Visit(predicate);


            return body;
        }

        private static DbTableExpression GetDbTable(Type type) => __instances.Select(builder => builder.table).Single(table => table.Type == type || table.Type.IsSubclassOf(type));

        private Expression GetDbColumnExpression(PropertyInfo propertyInfo)
        {
            DbTableExpression table = GetDbTable(propertyInfo.DeclaringType);

            foreach (DbColumnDeclaration column in table.Columns)
            {
                if (column.PropertyName == propertyInfo.Name)
                {
                    return column.Expression;
                }
            }
            return null;
        }

        private string GetName(string name, Type type = null)
        {
            if (type.IsNotNull())
            {
                if (type == typeof(DateTime))
                {
                    name = $"dt_{name}";
                }
            }

            return $"{name}_{parameters[DbExpressionType.Where].IsNotNull(x => x.Count) + 1}".ToLower(CultureInfo.CurrentCulture);
        }

        private Expression GetNamedExpression(object value)
        {
            DbTableExpression table = GetDbTable(propertyInfo.DeclaringType);

            IDbEntityProperty property = table.Model[propertyInfo.Name];

            string name = "";

            if (right is NewArrayExpression)
            {
                name = GetName("el", value.GetType());

                parameters.Add(whereType, new SubSonicParameter(property, $"@{name}") { Value = value });
            }
            else
            {
                name = GetName(property.Name, property.PropertyType);

                parameters.Add(whereType, new SubSonicParameter(property, $"@{name}") { Value = value });
            }

            return DbExpression.DbNamedValue(name,
                    Expression.Constant(Convert.ChangeType(value, propertyInfo.PropertyType, CultureInfo.CurrentCulture), propertyInfo.PropertyType));
        }
    }
}

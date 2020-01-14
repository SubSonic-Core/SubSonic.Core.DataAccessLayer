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

        private DbGroupOperator group;
        private DbComparisonOperator comparison;
        private DbExpressionType whereType;
        private PropertyInfo propertyInfo;
        private bool CanReadFromCache;
        private bool visitingForArray;

        protected DbWherePredicateBuilder(DbExpressionType whereType, DbTableExpression table)
        {
            parameters = new SubSonicParameterDictionary();
            Arguments = new ArgumentCollection<Expression>(whereType.ToString());
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
                return new DbWhereExpression(whereType, type, lambda, builder.ParseLambda(lambda), builder.CanReadFromCache, builder.parameters.ToReadOnlyCollection(DbExpressionType.Where));
            }
        }

        public ArgumentCollection<Expression> Arguments { get; }

        public static Expression GetComparisonExpression(Expression left, Expression right, DbComparisonOperator @operator)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            Expression result;

            switch (@operator)
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
                case DbComparisonOperator.In:
                    result = DbExpression.DbIn(left, right);
                    break;
                case DbComparisonOperator.NotIn:
                    result = DbExpression.DbNotIn(left, right);
                    break;
                default:
                    throw new NotImplementedException($"{@operator} operation is not implemented.");
            }

            return result;
        }

        public static Expression GetBodyExpression(Expression body, Expression right, DbGroupOperator @group)
        {
            if (body is null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            Expression result;

            switch (group)
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
                    CanReadFromCache |= column.Property.IsPrimaryKey;

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

        private Expression GetNamedExpression(PropertyInfo info, object value)
        {
            DbTableExpression table = GetDbTable(info.DeclaringType);

            IDbEntityProperty property = table.Model[info.Name];

            string name = "";

            if (visitingForArray)
            {
                name = GetName("el", value.GetType());

                parameters.Add(whereType, new SubSonicParameter($"@{name}", value, property));
            }
            else
            {
                name = GetName(property.Name, property.PropertyType);

                parameters.Add(whereType, new SubSonicParameter($"@{name}", value, property));
            }

            return DbExpression.DbNamedValue(name,
                    Expression.Constant(Convert.ChangeType(value, info.PropertyType, CultureInfo.CurrentCulture), info.PropertyType));
        }

        private Expression GetNamedExpression(FieldInfo info, object value)
        {
            string name = GetName(info.Name, info.FieldType);

            parameters.Add(whereType, new SubSonicParameter($"@{name}", value));

            return DbExpression.DbNamedValue(name,
                    Expression.Constant(Convert.ChangeType(value, info.FieldType, CultureInfo.CurrentCulture), info.FieldType));
        }
    }
}

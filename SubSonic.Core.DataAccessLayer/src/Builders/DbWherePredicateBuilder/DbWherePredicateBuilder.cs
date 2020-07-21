using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Builders
{
    using Schema;
    using Linq;
    using Linq.Expressions;
    using Linq.Expressions.Structure;
    using System.Data.Common;

    public partial class DbWherePredicateBuilder
        : DbExpressionVisitor
    {
        [ThreadStatic]
        private static Stack<DbWherePredicateBuilder> __instances;

        private readonly DbTableExpression table;
        private readonly SubSonicParameterDictionary parameters;
        private Expression body;

        private DbGroupOperator group;
        private DbComparisonOperator comparison;
        private readonly DbExpressionType whereType;
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

        public static Expression GetWhereTranslation(Expression expression)
        {
            if (expression is DbWhereExpression where)
            {
                Expression query = where.GetArgument(0);
                DbTableExpression dbTable = null;


                if (query is DbSelectExpression select)
                {
                    dbTable = select.From;
                }
                else if (query is DbSelectPageExpression paged_select)
                {
                    dbTable = paged_select.Select.From;
                }
                else if (query is DbSelectAggregateExpression aggregate_select)
                {
                    dbTable = aggregate_select.From;
                }
                else if (query is DbTableExpression table)
                {
                    dbTable = table;
                }
                else
                {
                    throw Error.NotSupported($"{query.GetType().Name} not supported");
                }

                using (var builder = new DbWherePredicateBuilder((DbExpressionType)where.NodeType, dbTable))
                {
                    return DbExpression.DbWhere(where.Method, where.Arguments,
                        builder.Visit(where),
                        builder.parameters.SelectMany(x => x.Value), 
                        builder.CanReadFromCache);
                }
            }

            return expression;
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

        private IEnumerable<DbTableExpression> DbTables => __instances.SelectMany(builder => builder.table.ToTableList());
            
        private DbTableExpression GetDbTable(Type type) => DbTables.Single(table =>
                    table.Type.GenericTypeArguments[0] == type || table.Type.GenericTypeArguments[0].IsSubclassOf(type));

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
                else if (type == typeof(Boolean))
                {
                    name = $"b_{name}";
                }
            }

            return $"{name}_{parameters[DbExpressionType.Where].IsNotNull(x => x.Count) + 1}".ToLower(CultureInfo.CurrentCulture);
        }

        private Expression GetNamedExpression(object value)
        {
            string name = GetName("value", value.GetType());

            parameters.Add(whereType, new SubSonicParameter($"@{name}", value));

            return DbExpression.DbNamedValue(name, Expression.Constant(value));
        }

        private Expression GetNamedExpression(object value, PropertyInfo info)
        {
            if (info.IsNotNull())
            {
                string name;

                DbTableExpression table = GetDbTable(info.DeclaringType);

                IDbEntityProperty property = table.Model[info.Name];

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

                return DbExpression.DbNamedValue(name, Expression.Constant(value, value.GetType()));
            }

            return GetNamedExpression(value);
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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MLinq = System.Linq;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using System.Net.Http.Headers;

    partial class DbWherePredicateBuilder
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.IsNotNull())
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        {
                            comparison = (DbComparisonOperator)Enum.Parse(typeof(DbComparisonOperator), node.NodeType.ToString());

                            Arguments.Push(Visit(node.Left));
                            Arguments.Push(Visit(node.Right));

                            BuildLogicalExpression();

                            
                        }
                        return node;
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        group = (DbGroupOperator)Enum.Parse(typeof(DbGroupOperator), node.NodeType.ToString());
                        break;
                }
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.IsNotNull())
            {
                if (node is MethodCallExpression call)
                {
                    if (Enum.TryParse(typeof(DbComparisonOperator), call.Method.Name, out object name))
                    {
                        comparison = (DbComparisonOperator)name;

                        if (comparison.In(DbComparisonOperator.In, DbComparisonOperator.NotIn))
                        {
                            foreach (Expression argument in call.Arguments)
                            {
                                if (argument is MethodCallExpression method)
                                {
                                    object set = Expression.Lambda(method).Compile().DynamicInvoke();

                                    Arguments.Push(PullUpParameters(((MLinq.IQueryable)set).Expression));
                                }
                                else
                                {
                                    Arguments.Push(Visit(argument));
                                }
                            }
                        }
                        else if (comparison.In(DbComparisonOperator.Between, DbComparisonOperator.NotBetween))
                        {
                            if (call.Method.Name.Contains("Between", StringComparison.CurrentCulture))
                            {
                                using (var args = Arguments.FocusOn(call.Method.Name))
                                {
                                    foreach (Expression argument in call.Arguments)
                                    {
                                        Arguments.Push(Visit(argument));
                                    }

                                    if (body.IsNull())
                                    {
                                        body = DbExpression.DbBetween(comparison, Arguments.Pop(), Arguments.Pop(), Arguments.Pop());
                                    }
                                    else
                                    {
                                        body = GetBodyExpression(body, DbExpression.DbBetween(comparison, Arguments.Pop(), Arguments.Pop(), Arguments.Pop()), group);
                                    }
                                }
                            }

                            return node;
                        }
                    }
                    else if (call.Method.Name.Contains("IsNull", StringComparison.CurrentCulture))
                    {
                        using (var args = Arguments.FocusOn(call.Method.Name))
                        {
                            foreach (Expression argument in call.Arguments)
                            {
                                Arguments.Push(Visit(argument));
                            }

                            return DbExpression.DbIsNull(args.Pop(), args.Pop());
                        }
                    }
                    else if (call.Method.GetCustomAttribute(typeof(DbProgrammabilityAttribute)) is DbScalarFunctionAttribute)
                    {
                        return DbExpression.DbScalar(call.Method.ReturnType, call, GetParameterExpressions(call));
                    }
                    else
                    {
                        return base.VisitMethodCall(node);
                    }

                    BuildLogicalExpression();

                    return node;
                }
            }
            return base.VisitMethodCall(node);
        }

        protected Expression[] GetParameterExpressions(MethodCallExpression call)
        {
            if (call.IsNotNull())
            {
                List<Expression> parameters = new List<Expression>();

                foreach(Expression argument in call.Arguments)
                {
                    parameters.Add(Visit(argument));

                    // scrub property info this is an arguments list not a right side value
                    propertyInfo = null;
                }

                return parameters.ToArray();
            }

            return Array.Empty<DbExpression>();
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node is LambdaExpression lambda)
            {
                if (whereType == DbExpressionType.Where)
                {
                    if(lambda.Body is MethodCallExpression call)
                    {
                        return VisitMethodCall(call);
                    }
                }
                else if (whereType.In(DbExpressionType.Exists, DbExpressionType.NotExists))
                {
                    object 
                        func = Expression.Lambda(lambda).Compile().DynamicInvoke(),
                        set = ((Delegate)func).DynamicInvoke(table.QueryObject);

                    body = PullUpParameters(((MLinq.IQueryable)set).Expression);

                    return node;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return base.VisitLambda(node);
        }

        private Expression PullUpParameters(Expression query)
        {
            if (query is DbSelectExpression select)
            {
                if (select.Where is DbWhereExpression where)
                {
                    parameters.AddRange((DbExpressionType)where.NodeType, where.Parameters.ToArray());
                }

                return select;
            }
            return null;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.IsNotNull())
            {
                switch (node.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        if (node.Member is PropertyInfo pi)
                        {
                            propertyInfo = pi;

                            return GetDbColumnExpression(pi);
                        }
                        else if (node.Member is FieldInfo fi)
                        {
                            if (node.Expression is ConstantExpression constant)
                            {
                                return GetNamedExpression(fi, fi.GetValue(constant.Value));
                            }

                            throw new NotSupportedException();
                        }
                        break;
                }
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            if(node is NewArrayExpression array)
            {
                List<Expression> elements = new List<Expression>();

                visitingForArray = true;

                foreach (Expression constant in array.Expressions)
                {
                    if (constant is ConstantExpression element)
                    {
                        elements.Add(VisitConstant(element));
                    }
                    else
                    {
                        elements.Add(Visit(constant));
                    }
                }

                visitingForArray = false;

                return array.Update(elements);
            }
            return base.VisitNewArray(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.IsNotNull())
            {
                return GetNamedExpression(node.Value, propertyInfo);
            }
            return base.VisitConstant(node);
        }

        protected virtual void BuildLogicalExpression()
        {
            if (Arguments.Count != 2)
            {
                return;
            }

            if (body.IsNull())
            {
                body = GetComparisonExpression(Arguments.Pop(), Arguments.Pop(), comparison);
            }
            else
            {
                body = GetBodyExpression(body, GetComparisonExpression(Arguments.Pop(), Arguments.Pop(), comparison), group);
            }
            // clear out the left right values in prep for the next one
            //left = right = null;
            propertyInfo = null;
        }
    }
}

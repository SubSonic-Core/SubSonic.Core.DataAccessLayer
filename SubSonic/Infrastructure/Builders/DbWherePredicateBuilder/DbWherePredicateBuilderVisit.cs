using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MLinq = System.Linq;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;

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
                        comparison = (ComparisonOperator)Enum.Parse(typeof(ComparisonOperator), node.NodeType.ToString());
                        break;
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        group = (GroupOperator)Enum.Parse(typeof(GroupOperator), node.NodeType.ToString());
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

                    if (Enum.TryParse(typeof(ComparisonOperator), call.Method.Name, out object name))
                    {
                        comparison = (ComparisonOperator)name;
                    }

                    if (comparison.In(ComparisonOperator.In, ComparisonOperator.NotIn))
                    {
                        foreach (Expression argument in call.Arguments)
                        {
                            if (argument is MethodCallExpression method)
                            {
                                object set = Expression.Lambda(method).Compile().DynamicInvoke();

                                right = PullUpParameters(((MLinq.IQueryable)set).Expression);
                            }
                            else
                            {
                                Visit(argument);
                            }
                        }
                    }
                    else if (comparison.In(ComparisonOperator.Between, ComparisonOperator.NotBetween))
                    {
                        foreach (Expression argument in call.Arguments)
                        {
                            Visit(argument);
                        }
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
                            if (left.IsNull())
                            {
                                propertyInfo = pi;

                                left = GetDbColumnExpression(pi);
                            }
                            else
                            {
                                right = GetDbColumnExpression(pi);

                                BuildLogicalExpression();
                            }

                            return node;
                        }
                        else if (node.Member is FieldInfo fi)
                        {
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

                right = array;

                foreach (Expression constant in array.Expressions)
                {
                    elements.Add(Visit(constant));
                }

                right = array.Update(elements);

                return node;
            }
            return base.VisitNewArray(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (right is NewArrayExpression array)
            {
                return GetNamedExpression(node.Value);
            }
            else if (!(right is DbColumnExpression))
            {
                right = GetNamedExpression(node.Value);
            }

            BuildLogicalExpression();

            return base.VisitConstant(node);
        }

        protected virtual void BuildLogicalExpression()
        {
            if (left is null || right is null)
            {
                throw new InvalidOperationException();
            }

            if (body.IsNull())
            {
                body = GetComparisonExpression(left, right, comparison);
            }
            else
            {
                body = GetBodyExpression(body, GetComparisonExpression(left, right, comparison), group);
            }
            // clear out the left right values in prep for the next one
            left = right = null;
            propertyInfo = null;
        }
    }
}

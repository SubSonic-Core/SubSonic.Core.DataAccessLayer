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
                    comparison = (ComparisonOperator)Enum.Parse(typeof(ComparisonOperator), call.Method.Name);

                    if (comparison.In(ComparisonOperator.In, ComparisonOperator.NotIn))
                    {
                        foreach (Expression argument in call.Arguments)
                        {
                            if (argument is MethodCallExpression method)
                            {
                                object set = Expression.Lambda(method).Compile().DynamicInvoke();

                                if (((MLinq.IQueryable)set).Expression is DbSelectExpression select)
                                {
                                    if (select.Where is DbWhereExpression where)
                                    {
                                        parameters.AddRange((DbExpressionType)where.NodeType, where.Parameters.ToArray());
                                    }

                                    right = select;
                                }
                            }
                            else
                            {
                                Visit(argument);
                            }
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    BuildLogicalExpression();

                    return node;
                }
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.IsNotNull())
            {
                switch(node.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        left = GetDbColumnExpression((PropertyInfo)node.Member);
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
            else
            {
                right = GetNamedExpression(node.Value);

                BuildLogicalExpression();
            }

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

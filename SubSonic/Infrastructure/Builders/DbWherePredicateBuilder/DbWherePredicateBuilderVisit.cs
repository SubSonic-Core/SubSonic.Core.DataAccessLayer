using SubSonic.Linq.Expressions;
using SubSonic.Linq.Expressions.Structure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure.Builders
{
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

        protected override Expression VisitIn(DbInExpression inExp)
        {
            if (inExp.IsNotNull())
            {
                switch ((DbExpressionType)inExp.NodeType)
                {
                    case DbExpressionType.In:
                    case DbExpressionType.NotIn:
                        comparison = (ComparisonOperator)Enum.Parse(typeof(ComparisonOperator), ((DbExpressionType)inExp.NodeType).ToString());
                        break;
                }
            }
            return base.VisitIn(inExp);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.IsNotNull())
            {
                if (node is MethodCallExpression call)
                {
                    comparison = (ComparisonOperator)Enum.Parse(typeof(ComparisonOperator), call.Method.Name);

                    foreach (Expression argument in call.Arguments)
                    {
                        Visit(argument);
                    }

                    if(call.Arguments[1] is DbSelectExpression select)
                    {
                        switch(comparison)
                        {
                            case ComparisonOperator.In:
                                right = new DbInExpression(node, select);
                                break;
                            case ComparisonOperator.NotIn:
                                right = new DbNotInExpression(node, select);
                                break;
                        }
                    }
                    else if (call.Arguments[1] is NewArrayExpression array)
                    {
                        switch (comparison)
                        {
                            case ComparisonOperator.In:
                                right = new DbInExpression(node, array);
                                break;
                            case ComparisonOperator.NotIn:
                                right = new DbNotInExpression(node, array);
                                break;
                        }
                    }

                    BuildLogicalExpression();
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

        protected override Expression VisitConstant(ConstantExpression node)
        {
            right = GetNamedExpression(node.Value);

            if (comparison != ComparisonOperator.In && comparison != ComparisonOperator.NotIn)
            {
                BuildLogicalExpression();
            }

            return base.VisitConstant(node);
        }

        protected virtual void BuildLogicalExpression()
        {
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

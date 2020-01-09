// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        protected string GetOperator(string methodName)
        {
            switch (methodName)
            {
                case "Add": return "+";
                case "Subtract": return "-";
                case "Multiply": return "*";
                case "Divide": return "/";
                case "Negate": return "-";
                case "Remainder": return "%";
                default: return null;
            }
        }

        private string GetOperator(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return "-";
                case ExpressionType.UnaryPlus:
                    return "+";
                case ExpressionType.Not:
                    return u.Operand.Type.IsBoolean() ? "NOT" : "~";
                default:
                    return "";
            }
        }

        private string GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return (b.Left.Type.IsBoolean()) ? "AND" : "&";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return (b.Left.Type.IsBoolean() ? "OR" : "|");
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ExclusiveOr:
                    return "^";
                default:
                    return "";
            }
        }

        private string GetOperator(DbExpression node)
        {
            switch((DbExpressionType)node.NodeType)
            {
                case DbExpressionType.In:
                    return context.Fragments.IN;
                case DbExpressionType.NotIn:
                    return context.Fragments.NOT_IN;
                case DbExpressionType.IsNull:
                    return context.Fragments.IS_NULL;
                case DbExpressionType.IsNotNull:
                    return context.Fragments.IS_NOT_NULL;
                case DbExpressionType.Between:
                    return context.Fragments.BETWEEN;
                case DbExpressionType.NotBetween:
                    return context.Fragments.NOT_BETWEEN;
                default:
                    return "";
            }
        }
    }
}

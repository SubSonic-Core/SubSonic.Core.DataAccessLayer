﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Alias;
    using SqlGenerator;
    using System.Reflection;

    public partial class TSqlFormatter
    {
        private bool IsPredicate(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return ((BinaryExpression)expr).Type.IsBoolean();
                case ExpressionType.Not:
                    return ((UnaryExpression)expr).Type.IsBoolean();
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case (ExpressionType)DbExpressionType.IsNull:
                case (ExpressionType)DbExpressionType.IsNotNull:
                case (ExpressionType)DbExpressionType.Between:
                case (ExpressionType)DbExpressionType.NotBetween:
                case (ExpressionType)DbExpressionType.In:
                case (ExpressionType)DbExpressionType.NotIn:
                    return expr.Type.IsBoolean();
                case ExpressionType.Call:
                    return ((MethodCallExpression)expr).Type.IsBoolean();
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expr).Type.IsBoolean();
                default:
                    return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual Expression VisitPredicate(Expression predicate)
        {
            if (predicate.IsNotNull())
            {
                this.Visit(predicate);

                if (!IsPredicate(predicate))
                {
                    Write(" <> 0");
                }
            }
            return predicate;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual Expression VisitValue(Expression value)
        {
            if (value.IsNotNull())
            {
                if (value is MemberExpression member)
                {
                    this.VisitMember(member);
                }
                else
                {
                    this.Visit(value);
                }
            }
            return value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitConditional(ConditionalExpression conditional)
        {
            if (conditional.IsNotNull())
            {
                if (this.IsPredicate(conditional.Test))
                {
                    Write("(CASE WHEN ");
                    this.VisitPredicate(conditional.Test);
                    Write(" THEN ");
                    this.VisitValue(conditional.IfTrue);
                    Expression ifFalse = conditional.IfFalse;
                    while (ifFalse != null && ifFalse.NodeType == ExpressionType.Conditional)
                    {
                        ConditionalExpression fc = (ConditionalExpression)ifFalse;
                        Write(" WHEN ");
                        this.VisitPredicate(fc.Test);
                        Write(" THEN ");
                        this.VisitValue(fc.IfTrue);
                        ifFalse = fc.IfFalse;
                    }
                    if (ifFalse != null)
                    {
                        Write(" ELSE ");
                        this.VisitValue(ifFalse);
                    }
                    Write(" END)");
                }
                else
                {
                    Write("(CASE ");
                    this.VisitValue(conditional.Test);
                    Write(" WHEN 0 THEN ");
                    this.VisitValue(conditional.IfFalse);
                    Write(" ELSE ");
                    this.VisitValue(conditional.IfTrue);
                    Write(" END)");
                }
            }
            return conditional;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (!(c is null))
            {

                if (c.Value is bool @bool)
                {
                    this.WriteValue(@bool ? 1 : 0);
                }
                else
                {
                    this.WriteValue(c.Value);
                }
            }

            return c;
        }
    }
}

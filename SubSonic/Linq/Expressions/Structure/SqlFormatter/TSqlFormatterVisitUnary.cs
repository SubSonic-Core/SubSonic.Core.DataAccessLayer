// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        protected override Expression VisitUnary(UnaryExpression unary)
        {
            if (unary.IsNotNull())
            {
                string op = GetOperator(unary);

                switch (unary.NodeType)
                {
                    case ExpressionType.Not:
                        if (unary.Operand.Type.IsBoolean())
                        {
                            Write(op);
                            Write(context.Fragments.SPACE);
                            this.VisitPredicate(unary.Operand);
                        }
                        else
                        {
                            Write(op);
                            this.VisitValue(unary.Operand);
                        }
                        break;
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        Write(op);
                        this.VisitValue(unary.Operand);
                        break;
                    case ExpressionType.UnaryPlus:
                        this.VisitValue(unary.Operand);
                        break;
                    case ExpressionType.Convert:
                        // ignore conversions for now
                        Visit(unary.Operand);
                        break;
                    default:
                        ThrowUnaryNotSupported(unary);
                        break;
                }
            }
            return unary;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        protected void ThrowUnaryNotSupported(UnaryExpression unary) => throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.UnSupportedUnaryOperator, $"{unary.NodeType}"));
    }
}

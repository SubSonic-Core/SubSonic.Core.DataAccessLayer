// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System;
using System.Globalization;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions.Structure
{
    using SubSonic.Core.DAL.src;

    public partial class TSqlFormatter
    {
        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if(binary.IsNotNull())
            {
                if (binary.Conversion.IsNotNull())
                {
                    binary = VisitAndConvert(binary, binary.Method.Name);
                }

                string op = GetOperator(binary);

                Expression
                    left = binary.Left,
                    right = binary.Right;

                if (binary.NodeType == ExpressionType.Power)
                {
                    Write($"POWER{Fragments.LEFT_PARENTHESIS}");
                    this.VisitValue(left);
                    Write($"{Fragments.COMMA} ");
                    this.VisitValue(right);
                    Write(Fragments.RIGHT_PARENTHESIS);

                    return binary;
                }
                else if (binary.NodeType == ExpressionType.Coalesce)
                {
                    Write($"COALESCE{Fragments.LEFT_PARENTHESIS}");
                    this.VisitValue(left);
                    Write($"{Fragments.COMMA} ");
                    while (right.NodeType == ExpressionType.Coalesce)
                    {
                        BinaryExpression rb = (BinaryExpression)right;
                        this.VisitValue(rb.Left);
                        Write($"{Fragments.COMMA} ");
                        right = rb.Right;
                    }
                    this.VisitValue(right);
                    Write(Fragments.RIGHT_PARENTHESIS);

                    return binary;
                }
                else
                {
                    Write(Fragments.LEFT_PARENTHESIS);
                    switch (binary.NodeType)
                    {
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                        case ExpressionType.Or:
                        case ExpressionType.OrElse:
                            if (left.Type.IsBoolean())
                            {
                                this.VisitPredicate(left);
                                Write(context.Fragments.SPACE);
                                Write(op);
                                Write(context.Fragments.SPACE);
                                this.VisitPredicate(right);
                            }
                            else
                            {
                                this.VisitValue(left);
                                Write(context.Fragments.SPACE);
                                Write(op);
                                Write(context.Fragments.SPACE);
                                this.VisitValue(right);
                            }
                            
                            break;
                        case ExpressionType.Equal:
                            if (right.NodeType == ExpressionType.Constant)
                            {
                                ConstantExpression ce = (ConstantExpression)right;
                                if (ce.Value == null)
                                {
                                    this.Visit(left);
                                    Write($" {Fragments.IS_NULL}");
                                    break;
                                }
                            }
                            else if (left.NodeType == ExpressionType.Constant)
                            {
                                ConstantExpression ce = (ConstantExpression)left;
                                if (ce.Value == null)
                                {
                                    this.Visit(right);
                                    Write($" {Fragments.IS_NULL}");
                                    break;
                                }
                            }
                            goto case ExpressionType.LessThan;
                        case ExpressionType.NotEqual:
                            if (right.NodeType == ExpressionType.Constant)
                            {
                                ConstantExpression ce = (ConstantExpression)right;
                                if (ce.Value == null)
                                {
                                    this.Visit(left);
                                    Write($" {Fragments.IS_NOT_NULL}");
                                    break;
                                }
                            }
                            else if (left.NodeType == ExpressionType.Constant)
                            {
                                ConstantExpression ce = (ConstantExpression)left;
                                if (ce.Value == null)
                                {
                                    this.Visit(right);
                                    Write($" {Fragments.IS_NOT_NULL}");
                                    break;
                                }
                            }
                            goto case ExpressionType.LessThan;
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                            // check for special x.CompareTo(y) && type.Compare(x,y)
                            if (left.NodeType == ExpressionType.Call && right.NodeType == ExpressionType.Constant)
                            {
                                MethodCallExpression mc = (MethodCallExpression)left;
                                ConstantExpression ce = (ConstantExpression)right;
                                if (ce.Value != null && ce.Value.GetType() == typeof(int) && ((int)ce.Value) == 0)
                                {
                                    if (mc.Method.Name == "CompareTo" && !mc.Method.IsStatic && mc.Arguments.Count == 1)
                                    {
                                        left = mc.Object;
                                        right = mc.Arguments[0];
                                    }
                                    else if (
                                        (mc.Method.DeclaringType == typeof(string) || mc.Method.DeclaringType == typeof(decimal))
                                          && mc.Method.Name == "Compare" && mc.Method.IsStatic && mc.Arguments.Count == 2)
                                    {
                                        left = mc.Arguments[0];
                                        right = mc.Arguments[1];
                                    }
                                }
                            }
                            goto case ExpressionType.Add;
                        case ExpressionType.Add:
                        case ExpressionType.AddChecked:
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractChecked:
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyChecked:
                        case ExpressionType.Divide:
                        case ExpressionType.Modulo:
                        case ExpressionType.ExclusiveOr:
                            this.VisitValue(left);
                            Write(context.Fragments.SPACE);
                            Write(op);
                            Write(context.Fragments.SPACE);
                            this.VisitValue(right);
                            break;
                        case ExpressionType.RightShift:
                            this.VisitValue(left);
                            Write($" / POWER{Fragments.LEFT_PARENTHESIS}2, ");
                            this.VisitValue(right);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            break;
                        case ExpressionType.LeftShift:
                            this.VisitValue(left);
                            Write($" * POWER{Fragments.LEFT_PARENTHESIS}2, ");
                            this.VisitValue(right);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            break;
                        default:
                            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.UnSupportedBinaryOperator, binary.NodeType));
                    }
                    Write(Fragments.RIGHT_PARENTHESIS);
                }

            }

            return binary;
        }
    }
}

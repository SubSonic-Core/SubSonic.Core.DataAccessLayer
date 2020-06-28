// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
// Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq.Microsoft
{
    using Expressions.Structure;
    using SubSonic.Linq.Expressions;

    public class ExpressionWriter
        : DbExpressionVisitor
    {
        private readonly TextWriter writer;
        private int depth;

        protected ExpressionWriter(TextWriter writer)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        protected int IndentationWidth { get; set; } = 2;

        protected void Indent(Indentation style)
        {
            if (style == Indentation.Inner)
            {
                depth++;
            }
            else if (style == Indentation.Outer)
            {
                depth--;
                System.Diagnostics.Debug.Assert(depth >= 0);
            }
        }

        protected void WriteLine(Indentation style)
        {
            writer.WriteLine();
            Indent(style);
            for (int i = 0, n = (depth * IndentationWidth); i < n; i++)
            {
                writer.Write(" ");
            }
        }

        protected void Write(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

#if NETSTANDARD2_0
            if (text.IndexOf('\n') >= 0)
#elif NETSTANDARD2_1
            if (text.IndexOf('\n', StringComparison.CurrentCulture) >= 0)
#endif
            {
                string[] lines = text.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0, n = lines.Length; i < n; i++)
                {
                    Write(lines[i]);
                    if (i < n - 1)
                    {
                        WriteLine(Indentation.Same);
                    }
                }
            }
            else
            {
                writer.Write(text);
            }
        }

        protected virtual string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Not:
                    return "!";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
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
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "&&";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "||";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Coalesce:
                    return "??";
                case ExpressionType.RightShift:
                    return ">>";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.ExclusiveOr:
                    return "^";
                default:
                    return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            switch (b.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    Visit(b.Left);
                    Write("[");
                    Visit(b.Right);
                    Write("]");
                    break;
                case ExpressionType.Power:
                    Write("POW(");
                    Visit(b.Left);
                    Write(", ");
                    Visit(b.Right);
                    Write(")");
                    break;
                default:
                    Visit(b.Left);
                    Write(" ");
                    Write(GetOperator(b.NodeType));
                    Write(" ");
                    Visit(b.Right);
                    break;
            }
            return b;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitUnary(UnaryExpression u)
        {
            if (u is null)
            {
                throw new ArgumentNullException(nameof(u));
            }

            switch (u.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    Write("((");
                    Write(u.Type.GetTypeName());
                    Write(")");
                    Visit(u.Operand);
                    Write(")");
                    break;
                case ExpressionType.ArrayLength:
                    Visit(u.Operand);
                    Write(".Length");
                    break;
                case ExpressionType.Quote:
                    Visit(u.Operand);
                    break;
                case ExpressionType.TypeAs:
                    Visit(u.Operand);
                    Write(" as ");
                    Write(u.Type.GetTypeName());
                    break;
                case ExpressionType.UnaryPlus:
                    Visit(u.Operand);
                    break;
                default:
                    Write(GetOperator(u.NodeType));
                    Visit(u.Operand);
                    break;
            }
            return u;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitConditional(ConditionalExpression c)
        {
            if (c is null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            Visit(c.Test);
            WriteLine(Indentation.Inner);
            Write("? ");
            Visit(c.IfTrue);
            WriteLine(Indentation.Same);
            Write(": ");
            Visit(c.IfFalse);
            Indent(Indentation.Outer);
            return c;
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            if (binding is null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(string.Format(CultureInfo.CurrentCulture, "Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            for (int i = 0, n = original.Count; i < n; i++)
            {
                VisitBinding(original[i]);
                if (i < n - 1)
                {
                    Write(",");
                    WriteLine(Indentation.Same);
                }
            }
            return original;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c is null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            if (c.Value == null)
            {
                Write("null");
            }
            else if (c.Type == typeof(string))
            {
                string value = c.Value.ToString();
                if (value.IndexOfAny(special) >= 0)
                    Write("@");
                Write("\"");
                Write(c.Value.ToString());
                Write("\"");
            }
            else if (c.Type == typeof(DateTime))
            {
                Write("new DataTime(\"");
                Write(c.Value.ToString());
                Write("\")");
            }
            else if (c.Type.IsArray)
            {
                Type elementType = c.Type.GetElementType();
                VisitNewArray(
                    Expression.NewArrayInit(
                        elementType,
                        ((IEnumerable)c.Value).OfType<object>().Select(v => (Expression)Expression.Constant(v, elementType))
                        ));
            }
            else
            {
                Write(c.Value.ToString());
            }
            return c;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            if (initializer != null)
            {
                if (initializer.Arguments.Count > 1)
                {
                    Write("{");
                    for (int i = 0, n = initializer.Arguments.Count; i < n; i++)
                    {
                        Visit(initializer.Arguments[i]);
                        if (i < n - 1)
                        {
                            Write(", ");
                        }
                    }
                    Write("}");
                }
                else
                {
                    Visit(initializer.Arguments[0]);
                }
            }
            return initializer;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            if (original != null)
            {
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    VisitElementInitializer(original[i]);
                    if (i < n - 1)
                    {
                        Write(",");
                        WriteLine(Indentation.Same);
                    }
                }
            }

            return original;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            if (original != null)
            {
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    Visit(original[i]);
                    if (i < n - 1)
                    {
                        Write(",");
                        WriteLine(Indentation.Same);
                    }
                }
            }
            return original;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            if (iv is null)
            {
                throw new ArgumentNullException(nameof(iv));
            }

            Write("Invoke(");
            WriteLine(Indentation.Inner);
            VisitExpressionList(iv.Arguments);
            Write(", ");
            WriteLine(Indentation.Same);
            Visit(iv.Expression);
            WriteLine(Indentation.Same);
            Write(")");
            Indent(Indentation.Outer);
            return iv;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            if (lambda is null)
            {
                throw new ArgumentNullException(nameof(lambda));
            }

            if (lambda.Parameters.Count > 1)
            {
                Write("(");
                for (int i = 0, n = lambda.Parameters.Count; i < n; i++)
                {
                    Write(lambda.Parameters[i].Name);
                    if (i < n - 1)
                    {
                        Write(", ");
                    }
                }
                Write(")");
            }
            else
            {
                Write(lambda.Parameters[0].Name);
            }
            Write(" => ");
            Visit(lambda.Body);
            return lambda;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitListInit(ListInitExpression init)
        {
            if (init is null)
            {
                throw new ArgumentNullException(nameof(init));
            }

            Visit(init.NewExpression);
            Write(" {");
            WriteLine(Indentation.Inner);
            VisitElementInitializerList(init.Initializers);
            WriteLine(Indentation.Outer);
            Write("}");
            return init;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            if (m is null)
            {
                throw new ArgumentNullException(nameof(m));
            }

            Visit(m.Expression);
            Write(".");
            Write(m.Member.Name);
            return m;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            if (assignment is null)
            {
                throw new ArgumentNullException(nameof(assignment));
            }

            Write(assignment.Member.Name);
            Write(" = ");
            Visit(assignment.Expression);
            return assignment;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitMemberInit(MemberInitExpression init)
        {
            if (init is null)
            {
                throw new ArgumentNullException(nameof(init));
            }

            Visit(init.NewExpression);
            Write(" {");
            WriteLine(Indentation.Inner);
            VisitBindingList(init.Bindings);
            WriteLine(Indentation.Outer);
            Write("}");
            return init;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            if (binding is null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            Write(binding.Member.Name);
            Write(" = {");
            WriteLine(Indentation.Inner);
            VisitElementInitializerList(binding.Initializers);
            WriteLine(Indentation.Outer);
            Write("}");
            return binding;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            if (binding is null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            Write(binding.Member.Name);
            Write(" = {");
            WriteLine(Indentation.Inner);
            VisitBindingList(binding.Bindings);
            WriteLine(Indentation.Outer);
            Write("}");
            return binding;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m is null)
            {
                throw new ArgumentNullException(nameof(m));
            }

            if (m.Object != null)
            {
                Visit(m.Object);
            }
            else
            {
                Write(m.Method.DeclaringType.GetTypeName());
            }
            Write(".");
            Write(m.Method.Name);
            Write("(");
            if (m.Arguments.Count > 1)
                WriteLine(Indentation.Inner);
            VisitExpressionList(m.Arguments);
            if (m.Arguments.Count > 1)
                WriteLine(Indentation.Outer);
            Write(")");
            return m;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitNew(NewExpression nex)
        {
            if (nex is null)
            {
                throw new ArgumentNullException(nameof(nex));
            }

            Write("new ");
            Write(nex.Constructor.DeclaringType.GetTypeName());
            Write("(");
            if (nex.Arguments.Count > 1)
                WriteLine(Indentation.Inner);
            VisitExpressionList(nex.Arguments);
            if (nex.Arguments.Count > 1)
                WriteLine(Indentation.Outer);
            Write(")");
            return nex;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected override Expression VisitNewArray(NewArrayExpression na)
        {
            if (na is null)
            {
                throw new ArgumentNullException(nameof(na));
            }

            Write("new ");
            Write(na.Type.GetElementType().GetTypeName());
            Write("[] {");
            if (na.Expressions.Count > 1)
                WriteLine(Indentation.Inner);
            VisitExpressionList(na.Expressions);
            if (na.Expressions.Count > 1)
                WriteLine(Indentation.Outer);
            Write("}");
            return na;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (p is null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            Write(p.Name);
            return p;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            Visit(b.Expression);
            Write(" is ");
            Write(b.TypeOperand.GetTypeName());
            return b;
        }

        protected virtual Expression VisitUnknown(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            Write(expression.ToString());
            return expression;
        }

        protected internal override Expression VisitInsert(DbInsertExpression insert)
        {
            throw new NotImplementedException();
        }

        protected internal override Expression VisitDelete(DbDeleteExpression delete)
        {
            throw new NotImplementedException();
        }
    }
}

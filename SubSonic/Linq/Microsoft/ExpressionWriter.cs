using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Microsoft
{
    public class ExpressionWriter
        : ExpressionVisitor
    {
        private static readonly char[] splitters = new char[] { '\n', '\r' };
        private static readonly char[] special = new char[] { '\n', '\n', '\\' };

        TextWriter writer;
        int indent = 2;
        int depth;

        protected enum Indentation
        {
            Same,
            Inner,
            Outer
        }

        protected ExpressionWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        protected int IndentationWidth
        {
            get { return indent; }
            set { indent = value; }
        }

        public static void Write(TextWriter writer, Expression expression)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            new ExpressionWriter(writer).Visit(expression);
        }

        public static string WriteToString(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            StringWriter sw = new StringWriter();
            Write(sw, expression);
            return sw.ToString();
        }

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
            for (int i = 0, n = depth * indent; i < n; i++)
            {
                writer.Write(" ");
            }
        }

        protected void Write(string text)
        {
            if (text.IndexOf('\n') >= 0)
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

        protected override Expression VisitBinary(BinaryExpression b)
        {
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

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    Write("((");
                    Write(GetTypeName(u.Type));
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
                    Write(GetTypeName(u.Type));
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

        protected virtual string GetTypeName(Type type)
        {
            string name = type.Name;
            name = name.Replace('+', '.');
            int iGeneneric = name.IndexOf('`');
            if (iGeneneric > 0)
            {
                name = name.Substring(0, iGeneneric);
            }
            if (type.IsGenericType || type.IsGenericTypeDefinition)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(name);
                sb.Append("<");
                var args = type.GetGenericArguments();
                for (int i = 0, n = args.Length; i < n; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    if (type.IsGenericType)
                    {
                        sb.Append(GetTypeName(args[i]));
                    }
                }
                sb.Append(">");
                name = sb.ToString();
            }
            return name;
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
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
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

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

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            if (initializer is null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }

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
            return initializer;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            for (int i = 0, n = original.Count; i < n; i++)
            {
                VisitElementInitializer(original[i]);
                if (i < n - 1)
                {
                    Write(",");
                    WriteLine(Indentation.Same);
                }
            }
            return original;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            for (int i = 0, n = original.Count; i < n; i++)
            {
                Visit(original[i]);
                if (i < n - 1)
                {
                    Write(",");
                    WriteLine(Indentation.Same);
                }
            }
            return original;
        }

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
                Write(GetTypeName(m.Method.DeclaringType));
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

        protected override Expression VisitNew(NewExpression nex)
        {
            if (nex is null)
            {
                throw new ArgumentNullException(nameof(nex));
            }

            Write("new ");
            Write(GetTypeName(nex.Constructor.DeclaringType));
            Write("(");
            if (nex.Arguments.Count > 1)
                WriteLine(Indentation.Inner);
            VisitExpressionList(nex.Arguments);
            if (nex.Arguments.Count > 1)
                WriteLine(Indentation.Outer);
            Write(")");
            return nex;
        }

        protected override Expression VisitNewArray(NewArrayExpression na)
        {
            if (na is null)
            {
                throw new ArgumentNullException(nameof(na));
            }

            Write("new ");
            Write(GetTypeName(na.Type.GetElementType()));
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

        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            Visit(b.Expression);
            Write(" is ");
            Write(GetTypeName(b.TypeOperand));
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Linq.Expressions.Structure
{
    using Infrastructure.SqlGenerator;

    public partial class TSqlFormatter
    {
        protected override Expression VisitMethodCall(MethodCallExpression method)
        {
            if (method.IsNotNull())
            {
                MethodInfo info = method.Method;

                if (info.DeclaringType == typeof(string))
                {
                    VisitStringMethods(info, method);
                }
                else if (info.DeclaringType == typeof(DateTime))
                {
                    VisitDateTimeMethods(info, method);
                }
                else if(info.DeclaringType.GetInterface(typeof(ISqlMethods).FullName).IsNotNull())
                {
                    VisitSqlMethods(info, method);
                }
                else
                {
                    ThrowMethodNotSupported(info);
                }

                return method;
            }

            return method;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitSqlMethods(MethodInfo info, MethodCallExpression method)
        {
            if (info.IsNotNull() && method.IsNotNull())
            {
                switch(info.Name)
                {
                    default:
                        ThrowMethodNotSupported(info);
                        return;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitDateTimeMethods(MethodInfo info, MethodCallExpression method)
        {
            if (info.IsNotNull() && method.IsNotNull())
            {
                switch (info.Name)
                {
                    case "op_Subtract":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        return;
                    default:
                        ThrowMethodNotSupported(info);
                        return;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitStringMethods(MethodInfo info, MethodCallExpression method)
        {
            if(info.IsNotNull() && method.IsNotNull())
            {
                switch (info.Name)
                {
                    case "StartsWith":
                        Write("(");
                        Visit(method.Object);
                        Write(" LIKE ");
                        Visit(method.Arguments[0]);
                        Write(" + '%')");
                        return;
                    case "EndsWith":
                        Write("(");
                        Visit(method.Object);
                        Write(" LIKE '%' + ");
                        Visit(method.Arguments[0]);
                        Write(")");
                        return;
                    case "Contains":
                        Write("(");
                        Visit(method.Object);
                        Write(" LIKE '%' + ");
                        Visit(method.Arguments[0]);
                        Write(" + '%')");
                        return;
                    case "Concat":
                        IList<Expression> args = method.Arguments;
                        if (args.Count == 1 && args[0].NodeType == ExpressionType.NewArrayInit)
                        {
                            args = ((NewArrayExpression)args[0]).Expressions;
                        }
                        for (int i = 0, n = args.Count; i < n; i++)
                        {
                            if (i > 0) Write(" + ");
                            Visit(args[i]);
                        }
                        return;
                    case "IsNullOrEmpty":
                        Write("(");
                        Visit(method.Arguments[0]);
                        Write(" IS NULL OR ");
                        Visit(method.Arguments[0]);
                        Write(" = '')");
                        return;
                    case "ToUpper":
                        Write("UPPER(");
                        Visit(method.Object);
                        Write(")");
                        return;
                    case "ToLower":
                        Write("LOWER(");
                        Visit(method.Object);
                        Write(")");
                        return;
                    case "Replace":
                        Write("REPLACE(");
                        Visit(method.Object);
                        Write(", ");
                        Visit(method.Arguments[0]);
                        Write(", ");
                        Visit(method.Arguments[1]);
                        Write(")");
                        return;
                    case "Substring":
                        Write("SUBSTRING(");
                        Visit(method.Object);
                        Write(", ");
                        Visit(method.Arguments[0]);
                        Write(" + 1, ");
                        if (method.Arguments.Count == 2)
                        {
                            Visit(method.Arguments[1]);
                        }
                        else
                        {
                            Write("8000");
                        }
                        Write(")");
                        return;
                    case "Remove":
                        Write("STUFF(");
                        Visit(method.Object);
                        Write(", ");
                        Visit(method.Arguments[0]);
                        Write(" + 1, ");
                        if (method.Arguments.Count == 2)
                        {
                            Visit(method.Arguments[1]);
                        }
                        else
                        {
                            Write("8000");
                        }
                        Write(", '')");
                        return;
                    case "IndexOf":
                        Write("(CHARINDEX(");
                        Visit(method.Object);
                        Write(", ");
                        Visit(method.Arguments[0]);
                        if (method.Arguments.Count == 2 && method.Arguments[1].Type == typeof(int))
                        {
                            Write(", ");
                            Visit(method.Arguments[1]);
                        }
                        Write(") - 1)");
                        return;
                    case "Trim":
                        Write("RTRIM(LTRIM(");
                        Visit(method.Object);
                        Write("))");
                        return;
                    default:
                        ThrowMethodNotSupported(info);
                        return;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        protected void ThrowMethodNotSupported(MemberInfo info) => throw new NotSupportedException(SubSonicErrorMessages.UnSupportedMethodException.Format($"{info.DeclaringType}.{info.Name}"));
    }
}

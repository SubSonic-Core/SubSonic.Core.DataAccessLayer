using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Linq.Expressions.Structure
{
    using Infrastructure.SqlGenerator;
    using System.Globalization;

    public partial class TSqlFormatter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
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
                else if (info.DeclaringType.GetInterface(typeof(ISqlMethods).FullName).IsNotNull())
                {
                    VisitSqlMethods(info, method);
                }
                else if (info.DeclaringType == typeof(Decimal))
                {
                    VisitDecimalMethods(info, method);
                }
                else if (info.DeclaringType == typeof(Math))
                {
                    VisitMathMethods(info, method);
                }
                else
                {
                    if (info.Name == "ToString")
                    {
                        if (method.Object.Type == typeof(string))
                        {
                            this.Visit(method.Object);  // no op
                        }
                        else
                        {
                            Write("CONVERT(VARCHAR(MAX), ");
                            this.Visit(method.Object);
                            Write(")");
                        }
                    }
                    else if (info.Name == "Equals")
                    {
                        if (info.IsStatic && info.DeclaringType == typeof(object))
                        {
                            Write("(");
                            this.Visit(method.Arguments[0]);
                            Write(sqlContext.SqlFragment.EQUAL_TO);
                            this.Visit(method.Arguments[1]);
                            Write(")");
                        }
                        else if (!info.IsStatic && method.Arguments.Count == 1 && method.Arguments[0].Type == method.Object.Type)
                        {
                            Write("(");
                            this.Visit(method.Object);
                            Write(sqlContext.SqlFragment.EQUAL_TO);
                            this.Visit(method.Arguments[0]);
                            Write(")");
                        }
                        else if (info.IsStatic && info.DeclaringType == typeof(string))
                        {
                            //Note: Not sure if this is best solution for fixing side issue with Issue #66
                            Write("(");
                            this.Visit(method.Arguments[0]);
                            Write(sqlContext.SqlFragment.EQUAL_TO);
                            this.Visit(method.Arguments[1]);
                            Write(")");
                        }
                    }
                    else
                    {
                        ThrowMethodNotSupported(info);
                    }
                }

                return method;
            }

            return method;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitMathMethods(MethodInfo info, MethodCallExpression method)
        {
            if (info.IsNotNull() && method.IsNotNull())
            {
                switch (info.Name)
                {
                    case "Abs":
                    case "Acos":
                    case "Asin":
                    case "Atan":
                    case "Cos":
                    case "Exp":
                    case "Log10":
                    case "Sin":
                    case "Tan":
                    case "Sqrt":
                    case "Sign":
                    case "Ceiling":
                    case "Floor":
                        Write(info.Name.ToUpper(CultureInfo.CurrentCulture));
                        Write("(");
                        this.Visit(method.Arguments[0]);
                        Write(")");
                        return;
                    case "Atan2":
                        Write("ATN2(");
                        this.Visit(method.Arguments[0]);
                        Write(", ");
                        this.Visit(method.Arguments[1]);
                        Write(")");
                        return;
                    case "Log":
                        if (method.Arguments.Count == 1)
                        {
                            goto case "Log10";
                        }
                        break;
                    case "Pow":
                        Write("POWER(");
                        this.Visit(method.Arguments[0]);
                        Write(", ");
                        this.Visit(method.Arguments[1]);
                        Write(")");
                        return;
                    case "Round":
                        if (method.Arguments.Count == 1)
                        {
                            Write("ROUND(");
                            this.Visit(method.Arguments[0]);
                            Write(", 0)");
                            return;
                        }
                        else if (method.Arguments.Count == 2 && method.Arguments[1].Type == typeof(int))
                        {
                            Write("ROUND(");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "Truncate":
                        Write("ROUND(");
                        this.Visit(method.Arguments[0]);
                        Write(", 0, 1)");
                        return;
                    default:
                        ThrowMethodNotSupported(info);
                        return;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitDecimalMethods(MethodInfo info, MethodCallExpression method)
        {
            if (info.IsNotNull() && method.IsNotNull())
            {
                switch (info.Name)
                {
                    case "Add":
                    case "Subtract":
                    case "Multiply":
                    case "Divide":
                    case "Remainder":
                        Write("(");
                        this.VisitValue(method.Arguments[0]);
                        Write(sqlContext.SqlFragment.SPACE);
                        Write(GetOperator(method.Method.Name));
                        Write(sqlContext.SqlFragment.SPACE);
                        this.VisitValue(method.Arguments[1]);
                        Write(")");
                        return;
                    case "Negate":
                        Write("-");
                        this.Visit(method.Arguments[0]);
                        Write("");
                        return;
                    case "Ceiling":
                    case "Floor":
                        Write(info.Name.ToUpper(CultureInfo.CurrentCulture));
                        Write("(");
                        this.Visit(method.Arguments[0]);
                        Write(")");
                        return;
                    case "Round":
                        if (method.Arguments.Count == 1)
                        {
                            Write("ROUND(");
                            this.Visit(method.Arguments[0]);
                            Write(", 0)");
                            return;
                        }
                        else if (method.Arguments.Count == 2 && method.Arguments[1].Type == typeof(int))
                        {
                            Write("ROUND(");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "Truncate":
                        Write("ROUND(");
                        this.Visit(method.Arguments[0]);
                        Write(", 0, 1)");
                        return;
                    default:
                        ThrowMethodNotSupported(info);
                        return;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitSqlMethods(MethodInfo info, MethodCallExpression method)
        {
            if (info.IsNotNull() && method.IsNotNull())
            {
                switch(info.Name)
                {
                    case "DateDiffDay":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(DAY,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "DateDiffHour":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(HOUR,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "DateDiffMicrosecond":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(MICROSECOND,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "DateDiffMillisecond":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(MILLISECOND,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "DateDiffMinute":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(MINUTE,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "DateDiffMonth":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(MONTH,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "DateDiffNanosecond":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(NANOSECOND,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "DateDiffSecond":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(SECOND,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
                    case "DateDiffYear":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(YEAR,");
                            this.Visit(method.Arguments[0]);
                            Write(", ");
                            this.Visit(method.Arguments[1]);
                            Write(")");
                            return;
                        }
                        break;
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

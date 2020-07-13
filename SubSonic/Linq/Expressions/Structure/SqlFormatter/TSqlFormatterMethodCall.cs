// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Linq.Expressions.Structure
{
    using Infrastructure;
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
                else if (info.DeclaringType == typeof(System.Linq.Queryable))
                {
                    VisitQueryableMethods(info, method);
                }
                else if (Linq.SubSonicQueryable.IsNotNull<Type>(info.DeclaringType.GetInterface(typeof(ISqlMethods).FullName)))
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
                            Write(Fragments.RIGHT_PARENTHESIS);
                        }
                    }
                    else if (info.Name == "Equals")
                    {
                        if (info.IsStatic && info.DeclaringType == typeof(object))
                        {
                            Write(Fragments.LEFT_PARENTHESIS);
                            this.Visit(method.Arguments[0]);
                            Write(context.Fragments.EQUAL_TO);
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                        }
                        else if (!info.IsStatic && method.Arguments.Count == 1 && method.Arguments[0].Type == method.Object.Type)
                        {
                            Write(Fragments.LEFT_PARENTHESIS);
                            this.Visit(method.Object);
                            Write(context.Fragments.EQUAL_TO);
                            this.Visit(method.Arguments[0]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                        }
                        else if (info.IsStatic && info.DeclaringType == typeof(string))
                        {
                            //Note: Not sure if this is best solution for fixing side issue with Issue #66
                            Write(Fragments.LEFT_PARENTHESIS);
                            this.Visit(method.Arguments[0]);
                            Write(context.Fragments.EQUAL_TO);
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                        }
                    }
                    else if (Linq.SubSonicQueryable.IsNotNull<Attribute>(info.GetCustomAttribute(typeof(DbProgrammabilityAttribute))))
                    {
                        if (info.GetCustomAttribute(typeof(DbProgrammabilityAttribute)) is DbScalarFunctionAttribute scalar)
                        {
                            Write(scalar.QualifiedName);
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

        protected virtual void VisitQueryableMethods(MethodInfo info, MethodCallExpression method)
        {
            if (info.IsNotNull() && method.IsNotNull())
            {
                switch(info.Name)
                {
                    case "Where":
                        Visit(method.Arguments[1]);
                        return;
                    default:
                        ThrowMethodNotSupported(info);
                        return;
                }
            }
        }

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
                        Write(Fragments.LEFT_PARENTHESIS);
                        this.Visit(method.Arguments[0]);
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "Atan2":
                        Write($"ATN2{Fragments.LEFT_PARENTHESIS}");
                        this.Visit(method.Arguments[0]);
                        Write($"{Fragments.COMMA} ");
                        this.Visit(method.Arguments[1]);
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "Log":
                        if (method.Arguments.Count == 1)
                        {
                            goto case "Log10";
                        }
                        break;
                    case "Pow":
                        Write($"POWER{Fragments.LEFT_PARENTHESIS}");
                        this.Visit(method.Arguments[0]);
                        Write($"{Fragments.COMMA} ");
                        this.Visit(method.Arguments[1]);
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "Round":
                        if (method.Arguments.Count == 1)
                        {
                            Write($"ROUND{Fragments.LEFT_PARENTHESIS}");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} 0{Fragments.RIGHT_PARENTHESIS}");
                            return;
                        }
                        else if (method.Arguments.Count == 2 && method.Arguments[1].Type == typeof(int))
                        {
                            Write($"ROUND{Fragments.LEFT_PARENTHESIS}");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "Truncate":
                        Write($"ROUND{Fragments.LEFT_PARENTHESIS}");
                        this.Visit(method.Arguments[0]);
                        Write($"{Fragments.COMMA} 0{Fragments.COMMA} 1{Fragments.RIGHT_PARENTHESIS}");
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
                        Write(Fragments.LEFT_PARENTHESIS);
                        this.VisitValue(method.Arguments[0]);
                        Write(context.Fragments.SPACE);
                        Write(GetOperator(method.Method.Name));
                        Write(context.Fragments.SPACE);
                        this.VisitValue(method.Arguments[1]);
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "Negate":
                        Write("-");
                        this.Visit(method.Arguments[0]);
                        Write("");
                        return;
                    case "Ceiling":
                    case "Floor":
                        Write(info.Name.ToUpper(CultureInfo.CurrentCulture));
                        Write(Fragments.LEFT_PARENTHESIS);
                        this.Visit(method.Arguments[0]);
                        Write(Fragments.RIGHT_PARENTHESIS);
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
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
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
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "DateDiffHour":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(HOUR,");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "DateDiffMicrosecond":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(MICROSECOND,");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "DateDiffMillisecond":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(MILLISECOND,");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "DateDiffMinute":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(MINUTE,");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "DateDiffMonth":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(MONTH,");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "DateDiffNanosecond":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(NANOSECOND,");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "DateDiffSecond":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(SECOND,");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
                            return;
                        }
                        break;
                    case "DateDiffYear":
                        if (method.Arguments[1].Type == typeof(DateTime))
                        {
                            Write("DATEDIFF(YEAR,");
                            this.Visit(method.Arguments[0]);
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
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
                            Write($"{Fragments.COMMA} ");
                            this.Visit(method.Arguments[1]);
                            Write(Fragments.RIGHT_PARENTHESIS);
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
                        Write(Fragments.LEFT_PARENTHESIS);
                        Visit(method.Object);
                        Write(" LIKE ");
                        Visit(method.Arguments[0]);
                        Write(" + '%')");
                        return;
                    case "EndsWith":
                        Write(Fragments.LEFT_PARENTHESIS);
                        Visit(method.Object);
                        Write(" LIKE '%' + ");
                        Visit(method.Arguments[0]);
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "Contains":
                        Write(Fragments.LEFT_PARENTHESIS);
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
                        Write(Fragments.LEFT_PARENTHESIS);
                        Visit(method.Arguments[0]);
                        Write(" IS NULL OR ");
                        Visit(method.Arguments[0]);
                        Write(" = '')");
                        return;
                    case "ToUpper":
                        Write("UPPER(");
                        Visit(method.Object);
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "ToLower":
                        Write("LOWER(");
                        Visit(method.Object);
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "Replace":
                        Write("REPLACE(");
                        Visit(method.Object);
                        Write($"{Fragments.COMMA} ");
                        Visit(method.Arguments[0]);
                        Write($"{Fragments.COMMA} ");
                        Visit(method.Arguments[1]);
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "Substring":
                        Write("SUBSTRING(");
                        Visit(method.Object);
                        Write($"{Fragments.COMMA} ");
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
                        Write(Fragments.RIGHT_PARENTHESIS);
                        return;
                    case "Remove":
                        Write("STUFF(");
                        Visit(method.Object);
                        Write($"{Fragments.COMMA} ");
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
                        Write($"{Fragments.COMMA} ");
                        Visit(method.Arguments[0]);
                        if (method.Arguments.Count == 2 && method.Arguments[1].Type == typeof(int))
                        {
                            Write($"{Fragments.COMMA} ");
                            Visit(method.Arguments[1]);
                        }
                        Write(") - 1)");
                        return;
                    case nameof(string.Trim):
                        Write("RTRIM(LTRIM(");
                        Visit(method.Object);
                        Write("))");
                        return;
                    case nameof(string.TrimEnd):
                        Write("RTRIM(");
                        Visit(method.Object);
                        Write(")");
                        return;
                    case nameof(string.TrimStart):
                        Write("LTRIM(");
                        Visit(method.Object);
                        Write(")");
                        return;
                    default:
                        ThrowMethodNotSupported(info);
                        return;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        protected void ThrowMethodNotSupported(MemberInfo info) => throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.UnSupportedMethodException, $"{info.DeclaringType}.{info.Name}"));
    }
}

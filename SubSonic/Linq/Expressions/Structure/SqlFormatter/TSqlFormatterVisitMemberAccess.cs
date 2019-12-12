using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Alias;
    using Infrastructure.SqlGenerator;
    using System.Linq;
    using System.Reflection;

    public partial class TSqlFormatter
    {
        protected virtual Expression VisitMemberAccess(MemberExpression member)
        {
            if(member.IsNotNull())
            {
                MemberInfo info = member.Member;

                if(info.DeclaringType == typeof(string))
                {
                    VisitStringMembers(info, member.Expression);
                }
                else if (new[] { typeof(DateTime), typeof(DateTimeOffset) }.Any(type => type == info.DeclaringType))
                {
                    VisitDateTimeMembers(info, member.Expression);
                }
                else
                {
                    throw new NotSupportedException(SubSonicErrorMessages.UnSupportedMemberException.Format(info.Name));
                }

                return member;
            }

            return member;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitDateTimeMembers(MemberInfo info, Expression expression)
        {
            if (info.IsNotNull() && expression.IsNotNull())
            {
                switch (info.Name)
                {
                    case "Day":
                        Write("DAY(");
                        this.Visit(expression);
                        Write(")");
                        return;
                    case "Month":
                        Write("MONTH(");
                        this.Visit(expression);
                        Write(")");
                        return;
                    case "Year":
                        Write("YEAR(");
                        this.Visit(expression);
                        Write(")");
                        return;
                    case "Hour":
                        Write("DATEPART(hour, ");
                        this.Visit(expression);
                        Write(")");
                        return;
                    case "Minute":
                        Write("DATEPART(minute, ");
                        this.Visit(expression);
                        Write(")");
                        return;
                    case "Second":
                        Write("DATEPART(second, ");
                        this.Visit(expression);
                        Write(")");
                        return;
                    case "Millisecond":
                        Write("DATEPART(millisecond, ");
                        this.Visit(expression);
                        Write(")");
                        return;
                    case "DayOfWeek":
                        Write("(DATEPART(weekday, ");
                        this.Visit(expression);
                        Write(") - 1)");
                        return;
                    case "DayOfYear":
                        Write("(DATEPART(dayofyear, ");
                        this.Visit(expression);
                        Write(") - 1)");
                        return;
                    default:
                        throw new NotSupportedException(SubSonicErrorMessages.UnSupportedMemberException.Format(info.Name));
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitStringMembers(MemberInfo info, Expression expression)
        {
            if(info.IsNotNull() && expression.IsNotNull())
            {
                switch (info.Name)
                {
                    case "Length":
                        {
                            Write("LEN(");
                            Visit(expression);
                            Write(")");
                        }
                        return;
                    default:
                        throw new NotSupportedException(SubSonicErrorMessages.UnSupportedMemberException.Format(info.Name));
                }
            }
        }
    }
}

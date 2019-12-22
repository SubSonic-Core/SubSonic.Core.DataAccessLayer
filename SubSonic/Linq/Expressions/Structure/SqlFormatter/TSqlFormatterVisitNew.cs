using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Alias;
    using Infrastructure.SqlGenerator;
    using System.Globalization;
    using System.Reflection;

    public partial class TSqlFormatter
    {
        protected override Expression VisitNew(NewExpression nex)
        {
            if (nex.IsNotNull())
            {
                if (nex.Constructor.DeclaringType == typeof(DateTime))
                {
                    VisitDateTimeConstructors(nex.Constructor, nex);
                }
                else
                {
                    ThrowConstructorNotSupported(nex.Constructor);
                }
            }

            return nex;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void VisitDateTimeConstructors(ConstructorInfo info, NewExpression nex)
        {
            if (info.IsNotNull() && nex.IsNotNull())
            {
                switch (nex.Arguments.Count)
                {
                    case 3:
                        Write("DATEADD(year, ");
                        this.Visit(nex.Arguments[0]);
                        Write(", DATEADD(month, ");
                        this.Visit(nex.Arguments[1]);
                        Write(", DATEADD(day, ");
                        this.Visit(nex.Arguments[2]);
                        Write(", 0)))");
                        return;
                    case 6:
                        Write("DATEADD(year, ");
                        this.Visit(nex.Arguments[0]);
                        Write(", DATEADD(month, ");
                        this.Visit(nex.Arguments[1]);
                        Write(", DATEADD(day, ");
                        this.Visit(nex.Arguments[2]);
                        Write(", DATEADD(hour, ");
                        this.Visit(nex.Arguments[3]);
                        Write(", DATEADD(minute, ");
                        this.Visit(nex.Arguments[4]);
                        Write(", DATEADD(second, ");
                        this.Visit(nex.Arguments[5]);
                        Write(", 0))))))");
                        return;
                    default:
                        ThrowConstructorNotSupported(info);
                        return;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        protected void ThrowConstructorNotSupported(ConstructorInfo info) => throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.UnSupportedConstructor, $"{info.DeclaringType}.{info.Name}"));
    }
}

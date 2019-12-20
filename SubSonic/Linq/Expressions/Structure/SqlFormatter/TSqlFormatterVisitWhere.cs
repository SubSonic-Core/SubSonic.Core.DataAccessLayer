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
        protected internal override Expression VisitWhere(DbWhereExpression where)
        {
            if (where.IsNotNull())
            {
                WriteNewLine();
                Write($"{Fragments.WHERE} ");

                if (!IsPredicate(where.Expression))
                {
                    Write(Fragments.LEFT_PARENTHESIS);
                    base.VisitWhere(where);
                    Write($"{Fragments.RIGHT_PARENTHESIS} <> 0");
                }
                else
                {
                    base.VisitWhere(where);
                }
            }
            return where;
        }
    }
}

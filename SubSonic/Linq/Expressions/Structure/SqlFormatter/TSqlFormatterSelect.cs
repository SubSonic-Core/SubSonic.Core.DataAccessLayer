using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Alias;
    using Infrastructure.SqlGenerator;
    using System.Reflection;

    public partial class TSqlFormatter
    {
        protected override Expression VisitSelect(DbSelectExpression select)
        {
            if (select.IsNotNull())
            {
                Write($"{context.Fragments.SELECT} ");
                if (select.IsDistinct)
                {
                    Write($"{context.Fragments.DISTINCT} ");
                }
                if (select.Take != null)
                {

                    Write($"{context.Fragments.TOP} {context.Fragments.LEFT_PARENTHESIS}");
                    this.Visit(select.Take);
                    Write($"{context.Fragments.RIGHT_PARENTHESIS} ");
                }
                if (select.Columns.Count > 0)
                {
                    for (int i = 0, n = select.Columns.Count; i < n; i++)
                    {
                        DbColumnDeclaration column = select.Columns[i];
                        if (i > 0)
                        {
                            Write($"{context.Fragments.COMMA} ");
                        }
                        DbColumnExpression c = this.VisitValue(column.Expression) as DbColumnExpression;
                        if (!string.IsNullOrEmpty(column.Name) && (c == null || c.Name != column.Name))
                        {
                            Write($" {context.Fragments.AS} ");
                            Write(column.Name);
                        }
                    }
                }
                else
                {
                    Write($"{context.Fragments.NULL} ");
                    if (IsNested)
                    {
                        Write($"{context.Fragments.AS} tmp ");
                    }
                }
                if (select.From != null)
                {
                    WriteNewLine();
                    Write($"{context.Fragments.FROM} ");
                    this.VisitSource(select.From);
                }
                if (select.Where != null)
                {
                    WriteNewLine();
                    Write($"{context.Fragments.WHERE} ");
                    this.VisitPredicate(select.Where);
                }
                if (select.GroupBy != null && select.GroupBy.Count > 0)
                {
                    WriteNewLine();
                    Write($"{context.Fragments.GROUP_BY} ");
                    for (int i = 0, n = select.GroupBy.Count; i < n; i++)
                    {
                        if (i > 0)
                        {
                            Write($"{context.Fragments.COMMA} ");
                        }
                        this.VisitValue(select.GroupBy[i]);
                    }
                }
                if (select.OrderBy != null && select.OrderBy.Count > 0)
                {
                    WriteNewLine();
                    Write($"{context.Fragments.ORDER_BY} ");
                    for (int i = 0, n = select.OrderBy.Count; i < n; i++)
                    {
                        DbOrderByDeclaration exp = select.OrderBy[i];
                        if (i > 0)
                        {
                            Write($"{context.Fragments.COMMA} ");
                        }
                        this.VisitValue(exp.Expression);
                        if (exp.OrderByType != OrderByType.Ascending)
                        {
                            Write($" {context.Fragments.DESC}");
                        }
                    }
                }
            }
            return select;
        }
    }
}

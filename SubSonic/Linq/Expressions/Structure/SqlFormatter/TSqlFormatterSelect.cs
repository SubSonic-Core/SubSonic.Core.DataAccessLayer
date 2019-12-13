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
                Write($"{Fragments.SELECT} ");
                if (select.IsDistinct)
                {
                    Write($"{Fragments.DISTINCT} ");
                }
                if (select.Take != null)
                {

                    Write($"{Fragments.TOP} {Fragments.LEFT_PARENTHESIS}");
                    this.Visit(select.Take);
                    Write($"{Fragments.RIGHT_PARENTHESIS} ");
                }
                if (select.Columns.Count > 0)
                {
                    for (int i = 0, n = select.Columns.Count; i < n; i++)
                    {
                        DbColumnDeclaration column = select.Columns[i];
                        if (i > 0)
                        {
                            Write($"{Fragments.COMMA} ");
                        }
                        DbColumnExpression c = this.VisitValue(column.Expression) as DbColumnExpression;
                        if (!string.IsNullOrEmpty(column.PropertyName) && (c == null || c.Name != column.PropertyName))
                        {
                            Write($" {Fragments.AS} ");
                            Write($"[{column.PropertyName}]");
                        }
                    }
                }
                else
                {
                    Write($"{Fragments.NULL} ");
                    if (IsNested)
                    {
                        Write($"{Fragments.AS} tmp ");
                    }
                }
                if (select.From != null)
                {
                    WriteNewLine();
                    Write($"{Fragments.FROM} ");
                    this.VisitSource(select.From);
                }
                if (select.Where != null)
                {
                    WriteNewLine();
                    Write($"{Fragments.WHERE} ");
                    this.VisitPredicate(select.Where);
                }
                if (select.GroupBy != null && select.GroupBy.Count > 0)
                {
                    WriteNewLine();
                    Write($"{Fragments.GROUP_BY} ");
                    for (int i = 0, n = select.GroupBy.Count; i < n; i++)
                    {
                        if (i > 0)
                        {
                            Write($"{Fragments.COMMA} ");
                        }
                        this.VisitValue(select.GroupBy[i]);
                    }
                }
                if (select.OrderBy != null && select.OrderBy.Count > 0)
                {
                    WriteNewLine();
                    Write($"{Fragments.ORDER_BY} ");
                    for (int i = 0, n = select.OrderBy.Count; i < n; i++)
                    {
                        DbOrderByDeclaration exp = select.OrderBy[i];
                        if (i > 0)
                        {
                            Write($"{Fragments.COMMA} ");
                        }
                        this.VisitValue(exp.Expression);
                        if (exp.OrderByType != OrderByType.Ascending)
                        {
                            Write($" {Fragments.DESC}");
                        }
                    }
                }
            }
            return select;
        }
    }
}

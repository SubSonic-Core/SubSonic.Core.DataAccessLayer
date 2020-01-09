// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions.Structure
{
    using System.Linq;

    public partial class TSqlFormatter
    {
        protected internal override Expression VisitSelect(DbSelectExpression select)
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
                        DbColumnDeclaration column = select.Columns.ElementAt(i);
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
                    VisitSource(select.From);
                }
                if (select.Where != null)
                {
                    Visit(select.Where);
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

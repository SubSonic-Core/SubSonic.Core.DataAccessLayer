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
                Write("SELECT ");
                if (select.IsDistinct)
                {
                    Write("DISTINCT ");
                }
                if (select.Take != null)
                {

                    Write("TOP (");
                    this.Visit(select.Take);
                    Write(") ");
                }
                if (select.Columns.Count > 0)
                {
                    for (int i = 0, n = select.Columns.Count; i < n; i++)
                    {
                        DbColumnDeclaration column = select.Columns[i];
                        if (i > 0)
                        {
                            Write(", ");
                        }
                        DbColumnExpression c = this.VisitValue(column.Expression) as DbColumnExpression;
                        if (!string.IsNullOrEmpty(column.Name) && (c == null || c.Name != column.Name))
                        {
                            Write(" AS ");
                            Write(column.Name);
                        }
                    }
                }
                else
                {
                    Write("NULL ");
                    if (IsNested)
                    {
                        Write("AS tmp ");
                    }
                }
                if (select.From != null)
                {
                    WriteNewLine(Indentation.Same);
                    Write("FROM ");
                    this.VisitSource(select.From);
                }
                if (select.Where != null)
                {
                    WriteNewLine(Indentation.Same);
                    Write("WHERE ");
                    this.VisitPredicate(select.Where);
                }
                if (select.GroupBy != null && select.GroupBy.Count > 0)
                {
                    WriteNewLine(Indentation.Same);
                    Write("GROUP BY ");
                    for (int i = 0, n = select.GroupBy.Count; i < n; i++)
                    {
                        if (i > 0)
                        {
                            Write(", ");
                        }
                        this.VisitValue(select.GroupBy[i]);
                    }
                }
                if (select.OrderBy != null && select.OrderBy.Count > 0)
                {
                    WriteNewLine(Indentation.Same);
                    Write("ORDER BY ");
                    for (int i = 0, n = select.OrderBy.Count; i < n; i++)
                    {
                        DbOrderByDeclaration exp = select.OrderBy[i];
                        if (i > 0)
                        {
                            Write(", ");
                        }
                        this.VisitValue(exp.Expression);
                        if (exp.OrderByType != OrderByType.Ascending)
                        {
                            Write(" DESC");
                        }
                    }
                }
            }
            return select;
        }
    }
}

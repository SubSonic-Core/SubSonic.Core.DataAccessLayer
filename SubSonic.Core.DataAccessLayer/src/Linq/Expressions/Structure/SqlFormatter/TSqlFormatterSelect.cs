// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System.Linq.Expressions;
using System;

namespace SubSonic.Linq.Expressions.Structure
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class TSqlFormatter
    {
        protected internal override Expression VisitSelect(DbExpression expression)
        {
            if (expression is DbSelectExpression select)
            {
                return VisitSelect(select);
            }
            else if (expression is DbSelectPageExpression pagedSelect)
            {
                return VisitSelect(pagedSelect);
            }
            else if (expression is DbSelectAggregateExpression aggregateSelect)
            {
                return VisitSelect(aggregateSelect);
            }

            return expression;
        }

        protected DbExpression VisitSelect(DbSelectAggregateExpression aggregate)
        {
            if(aggregate.IsNotNull())
            {
                Write($"{Fragments.SELECT} ");
                if (aggregate.Columns.Count > 0)
                {
                    for (int i = 0, n = aggregate.Columns.Count; i < n; i++)
                    {
                        if (i > 0)
                        {
                            Write($"{Fragments.COMMA} ");
                        }

                        if (aggregate.Columns[i] is DbColumnAggregateExpression column)
                        {
                            VisitValue(column.Argument);

                            Write($" [{column.Name}]");
                        }
                        else
                        {
                            VisitValue(aggregate.Columns[i]);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
                if (aggregate.From != null)
                {
                    WriteNewLine();
                    Write($"{Fragments.FROM} ");

                    VisitSource(aggregate.From);

                    foreach (DbExpression expression in aggregate.From.Joins)
                    {
                        VisitSource(expression, aggregate.IsCte);
                    }
                }
                if (aggregate.Where != null)
                {
                    Visit(aggregate.Where);
                }
                if (aggregate.GroupBy != null && aggregate.GroupBy.Count > 0)
                {
                    WriteNewLine();
                    Write($"{Fragments.GROUP_BY} ");
                    for (int i = 0, n = aggregate.GroupBy.Count; i < n; i++)
                    {
                        if (i > 0)
                        {
                            Write($"{Fragments.COMMA} ");
                        }
                        this.VisitValue(aggregate.GroupBy[i]);
                    }
                }
            }

            return aggregate;
        }

        protected DbExpression VisitSelect(DbSelectPageExpression paged)
        {
            if (paged.IsNotNull())
            {
                WriteNewLine($"{Fragments.WITH} {paged.PageCte.QualifiedName} {Fragments.AS}");
                WriteNewLine(Fragments.LEFT_PARENTHESIS);
                WriteNewLine(Indentation.Inner);
                this.VisitSelect(paged.PrimaryKeySelect);
                WriteNewLine();
                WriteNewLine($"{Fragments.OFFSET} @{nameof(paged.PageSize)} * {Fragments.LEFT_PARENTHESIS}@{nameof(paged.PageNumber)} - 1{Fragments.RIGHT_PARENTHESIS} {Fragments.ROWS}");
                WriteNewLine($"{Fragments.FETCH} {Fragments.NEXT} @{nameof(paged.PageSize)} {Fragments.ROWS} {Fragments.ONLY}", Indentation.Outer);
                WriteNewLine(Fragments.RIGHT_PARENTHESIS);
                Write($"{Fragments.SELECT} ");
                if (paged.Select.IsDistinct)
                {
                    Write($"{Fragments.DISTINCT} ");
                }
                if (paged.Select.Take != null)
                {
                    Write($"{Fragments.TOP} {Fragments.LEFT_PARENTHESIS}");
                    this.Visit(paged.Select.Take);
                    Write($"{Fragments.RIGHT_PARENTHESIS} ");
                }
                if (paged.Select.Columns.Count > 0)
                {
                    WriteColumnList(paged.Select.Columns.OrderBy(x => x.Order));
                }
                else
                {
                    Write($"[{GetAliasName(paged.Select.Alias)}].{Fragments.ASTRIX}");
                }
                if (paged.Select.From != null)
                {
                    WriteNewLine();
                    Write($"{Fragments.FROM} ");

                    VisitSource(paged.Select.From);

                    foreach (DbExpression expression in paged.Select.From.Joins)
                    {
                        VisitSource(expression);
                    }
                }
                if (paged.Select.GroupBy != null && paged.Select.GroupBy.Count > 0)
                {
                    WriteNewLine();
                    Write($"{Fragments.GROUP_BY} ");
                    for (int i = 0, n = paged.Select.GroupBy.Count; i < n; i++)
                    {
                        if (i > 0)
                        {
                            Write($"{Fragments.COMMA} ");
                        }
                        this.VisitValue(paged.Select.GroupBy[i]);
                    }
                }
                
                WriteNewLine();
                Write($"{Fragments.OPTION} {Fragments.LEFT_PARENTHESIS}{Fragments.RECOMPILE}{Fragments.RIGHT_PARENTHESIS}");
            }

            return paged;
        }

        protected DbExpression VisitSelect(DbSelectExpression select)
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
                    WriteColumnList(select.Columns.OrderBy(x => x.Order));
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
                    foreach (DbExpression expression in select.From.Joins)
                    {
                        VisitSource(expression, select.IsCte);
                    }
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

        private void WriteColumnList(IEnumerable<DbColumnDeclaration> columns)
        {
            bool first = true;

            foreach (DbColumnDeclaration column in columns)
            {
                if (!first)
                {
                    Write($"{Fragments.COMMA} ");
                }
                DbColumnExpression c = this.VisitValue(column.Expression) as DbColumnExpression;
                if (!string.IsNullOrEmpty(column.PropertyName) && (c == null || c.Name != column.PropertyName))
                {
                    Write($" {Fragments.AS} ");
                    Write($"[{column.PropertyName}]");
                }

                first = false;
            }
        }
    }
}

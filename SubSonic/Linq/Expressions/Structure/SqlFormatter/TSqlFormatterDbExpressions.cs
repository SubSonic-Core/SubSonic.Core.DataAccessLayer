// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        protected internal override DbExpression VisitExpression(DbExpression expression)
        {
            if (expression.IsNotNull())
            {
                if (expression.IsOfType<DbColumnExpression>())
                {
                    return VisitColumn((DbColumnExpression)expression);
                }
                else if (expression.IsOfType<DbNamedValueExpression>())
                {
                    return VisitNamedValue((DbNamedValueExpression)expression);
                }
            }
            return expression;
        }

        protected virtual DbExpression VisitNamedValue(DbNamedValueExpression value)
        {
            if (value.IsNotNull())
            {
                Write($"@{value.Name}");
            }

            return value;
        }

        protected virtual DbExpression VisitColumn(DbColumnExpression expression)
        {
            if (expression.IsNotNull())
            {
                if (expression.Alias != null)
                {
                    Write($"[{GetAliasName(expression.Alias)}].");
                }
                WriteFormat($"[{expression.Name}]");
            }
            return expression;
        }

        protected internal override Expression VisitProjection(DbProjectionExpression projection)
        {
            if (projection.IsNotNull())
            {
                // treat these like scalar subqueries
                if (projection.Projector is DbColumnExpression)
                {
                    Write(context.Fragments.LEFT_PARENTHESIS);
                    WriteNewLine(Indentation.Inner);
                    this.Visit(projection.Source);
                    Write(context.Fragments.RIGHT_PARENTHESIS);
                    this.Indent(Indentation.Outer);
                }
                else
                {
                    throw new NotSupportedException(SubSonicErrorMessages.NonScalarProjection);
                }
            }
            return projection;
        }

        protected internal override Expression VisitJoin(DbJoinExpression join, bool cte = false)
        {
            if (join.IsNotNull())
            {
                if (join.Right is DbTableExpression table)
                {
                    if (cte && table.IsNamedAlias)
                    {
                        return join;
                    }

                    WriteNewLine(Indentation.Inner);

                    switch (join.Join)
                    {
                        case JoinType.CrossJoin:
                            Write($"{context.Fragments.CROSS_JOIN} ");
                            break;
                        case JoinType.InnerJoin:
                            Write($"{context.Fragments.INNER_JOIN} ");
                            break;
                        case JoinType.CrossApply:
                            Write($"{context.Fragments.CROSS_APPLY} ");
                            break;
                        case JoinType.OuterApply:
                            Write($"{context.Fragments.OUTER_APPLY} ");
                            break;
                        case JoinType.LeftOuter:
                            Write($"{context.Fragments.LEFT_OUTER_JOIN} ");
                            break;
                    }

                    this.VisitSource(join.Right);

                    if (join.Condition != null)
                    {
                        WriteNewLine(Indentation.Inner);
                        Write($"{Fragments.ON} ");
                        VisitPredicate(join.Condition);
                        Indent(Indentation.Outer);
                    }
                }
            }
            return join;
        }

        protected override Expression VisitSource(Expression source, bool cte = false)
        {
            if (source.IsNotNull())
            {
                bool saveIsNested = IsNested;
                IsNested = true;

                if(source is DbTableExpression DbTable)
                {
                    Write(DbTable.QualifiedName);

                    if (!DbTable.IsNamedAlias)
                    {
                        Write($" {context.Fragments.AS} ");
                        Write($"[{GetAliasName(DbTable.Alias)}]");
                    }
                }
                else if (source is DbSelectExpression select)
                {
                    WriteNewLine(Fragments.LEFT_PARENTHESIS);
                    WriteNewLine(Indentation.Inner);
                    this.Visit(select);
                    WriteNewLine(Indentation.Outer);
                    WriteNewLine(Fragments.RIGHT_PARENTHESIS);
                }
                else if (source is DbJoinExpression join)
                {
                    this.VisitJoin(join, cte);
                }
                else
                {
                    throw new InvalidOperationException(SubSonicErrorMessages.SelectSourceIsNotValid);
                }
                IsNested = saveIsNested;
            }
            return source;
        }

        protected internal override Expression VisitAggregate(DbAggregateExpression aggregate)
        {
            if (aggregate.IsNotNull())
            {
                Write(GetAggregateName(aggregate.AggregateType));
                Write(context.Fragments.LEFT_PARENTHESIS);
                if (aggregate.IsDistinct)
                {
                    Write(context.Fragments.DISTINCT);
                }
                if (aggregate.Argument != null)
                {
                    VisitValue(aggregate.Argument);
                }
                else if (RequiresAsteriskWhenNoArgument(aggregate.AggregateType))
                {
                    Write(context.Fragments.ASTRIX);
                }
                Write(context.Fragments.RIGHT_PARENTHESIS);
            }
            return aggregate;
        }

        protected internal override Expression VisitNull(DbIsNullExpression isNull)
        {
            if (isNull.IsNotNull())
            {
                this.VisitValue(isNull.Expression);

                Write($" {GetOperator(isNull)}");
            }
            return isNull;
        }

        protected internal override Expression VisitBetween(DbBetweenExpression between)
        {
            if (between.IsNotNull())
            {
                this.VisitValue(between.Value);
                Write($" {GetOperator(between)} ");
                this.VisitValue(between.Lower);
                Write($" {Fragments.AND} ");
                this.VisitValue(between.Upper);
            }
            return between;
        }

        protected internal override Expression VisitRowNumber(DbRowNumberExpression rowNumber)
        {
            if (rowNumber.IsNotNull())
            {
                Write(Fragments.ROW_NUMBER);
                Write(Fragments.RIGHT_PARENTHESIS);
                if (rowNumber.OrderBy != null && rowNumber.OrderBy.Count > 0)
                {
                    Write(context.Fragments.ORDER_BY);
                    for (int i = 0, n = rowNumber.OrderBy.Count; i < n; i++)
                    {
                        DbOrderByDeclaration exp = rowNumber.OrderBy[i];
                        if (i > 0)
                        {
                            Write($"{Fragments.COMMA} ");
                        }
                        this.VisitValue(exp.Expression);
                        if (exp.OrderByType != OrderByType.Ascending)
                        {
                            Write(context.Fragments.DESC);
                        }
                    }
                }
                Write(Fragments.LEFT_PARENTHESIS);
            }
            return rowNumber;
        }

        protected internal override Expression VisitScalar(DbScalarExpression subquery)
        {
            if (subquery.IsNotNull())
            {
                this.Write(Fragments.RIGHT_PARENTHESIS);
                WriteNewLine(Indentation.Inner);
                this.Visit(subquery.Select);
                WriteNewLine();
                Write(Fragments.LEFT_PARENTHESIS);
                this.Indent(Indentation.Outer);
            }

            return subquery;
        }

        protected internal override Expression VisitIn(DbInExpression @in)
        {
            if (@in.IsNotNull())
            {
                this.VisitValue(@in.Left);
                
                switch((DbExpressionType)@in.NodeType)
                {
                    case DbExpressionType.In:
                        Write($" {Fragments.IN} {Fragments.LEFT_PARENTHESIS}");
                        break;
                    case DbExpressionType.NotIn:
                        Write($" {Fragments.NOT_IN} {Fragments.LEFT_PARENTHESIS}");
                        break;
                }

                if (@in.Inside is DbSelectExpression select)
                {
                    WriteNewLine(Indentation.Inner);
                    this.Visit(select);
                    Write(Fragments.RIGHT_PARENTHESIS);
                    this.Indent(Indentation.Outer);
                }
                else if (@in.Inside is NewArrayExpression array)
                {
                    for(int i = 0; i < array.Expressions.Count; i++)
                    {
                        Visit(array.Expressions[i]);
                        if (i < (array.Expressions.Count - 1))
                        {
                            Write($"{Fragments.COMMA} ");
                        }
                    }
                    Write(Fragments.RIGHT_PARENTHESIS);
                }
            }
            return @in;
        }

        
    }
}

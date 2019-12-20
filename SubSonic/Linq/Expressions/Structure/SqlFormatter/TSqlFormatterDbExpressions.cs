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

        protected override Expression VisitProjection(DbProjectionExpression projection)
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

        protected override Expression VisitJoin(DbJoinExpression join)
        {
            if (join.IsNotNull())
            {
                this.VisitSource(join.Left);

                WriteNewLine();

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
                    Write(context.Fragments.ON);
                    this.VisitPredicate(join.Condition);
                    this.Indent(Indentation.Outer);
                }
            }
            return join;
        }

        protected override Expression VisitSource(Expression source)
        {
            if (source.IsNotNull())
            {
                bool saveIsNested = IsNested;
                IsNested = true;

                if(source is DbTableExpression DbTable)
                {
                    Write(DbTable.Model);
                    Write($" {context.Fragments.AS} ");
                    Write($"[{GetAliasName(DbTable.Table)}]");
                }
                else if (source is DbSelectExpression select)
                {
                    Write(context.Fragments.LEFT_PARENTHESIS);
                    WriteNewLine(Indentation.Inner);
                    this.Visit(select);
                    WriteNewLine();
                    Write(context.Fragments.RIGHT_PARENTHESIS);
                    Write($" {context.Fragments.AS} ");
                    Write(GetAliasName(select.Table));
                    this.Indent(Indentation.Outer);
                }
                else if (source is DbJoinExpression join)
                {
                    this.VisitJoin(join);
                }
                else
                {
                    throw new InvalidOperationException(SubSonicErrorMessages.SelectSourceIsNotValid);
                }
                IsNested = saveIsNested;
            }
            return source;
        }

        protected override Expression VisitAggregate(DbAggregateExpression aggregate)
        {
            if (aggregate.IsNotNull())
            {
                Write(GetAggregateName(aggregate.AggregateType));
                Write(context.Fragments.RIGHT_PARENTHESIS);
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
                Write(context.Fragments.LEFT_PARENTHESIS);
            }
            return aggregate;
        }

        protected override Expression VisitNull(DbIsNullExpression isNull)
        {
            if (isNull.IsNotNull())
            {
                this.VisitValue(isNull.Expression);

                Write($" {GetOperator(isNull)}");
            }
            return isNull;
        }

        protected override Expression VisitBetween(DbBetweenExpression between)
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

        protected override Expression VisitRowNumber(DbRowNumberExpression rowNumber)
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

        protected override Expression VisitScalar(DbScalarExpression subquery)
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

        protected override Expression VisitExists(DbExistsExpression exists)
        {
            if (exists.IsNotNull())
            {
                Write($"{Fragments.EXISTS}{Fragments.RIGHT_PARENTHESIS}");
                WriteNewLine(Indentation.Inner);
                this.Visit(exists.Select);
                WriteNewLine(Indentation.Same);
                Write(Fragments.LEFT_PARENTHESIS);
                this.Indent(Indentation.Outer);
            }
            return exists;
        }

        protected override Expression VisitIn(DbInExpression @in)
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

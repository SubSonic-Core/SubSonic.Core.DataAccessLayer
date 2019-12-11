using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    public abstract class DbExpressionVisitorOld 
        : ExpressionVisitor
    {
        public override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            switch ((DbExpressionType)exp.NodeType)
            {
                case DbExpressionType.Table:
                    return this.VisitTable((DbTableExpression)exp);
                case DbExpressionType.Column:
                    return this.VisitColumn((DbColumnExpression)exp);
                case DbExpressionType.Select:
                    return this.VisitSelect((DbSelectExpression)exp);
                case DbExpressionType.Join:
                    return this.VisitJoin((DbJoinExpression)exp);
                case DbExpressionType.OuterJoined:
                    return this.VisitOuterJoined((DbOuterJoinedExpression)exp);
                case DbExpressionType.Aggregate:
                    return this.VisitAggregate((DbAggregateExpression)exp);
                case DbExpressionType.Scalar:
                case DbExpressionType.Exists:
                case DbExpressionType.In:
                case DbExpressionType.AggregateSubQuery:
                    return this.VisitAggregateSubquery((DbAggregateSubQueryExpression)exp);
                case DbExpressionType.IsNull:
                    return this.VisitIsNull((DbIsNullExpression)exp);
                case DbExpressionType.Between:
                    return this.VisitBetween((DbBetweenExpression)exp);
                case DbExpressionType.RowCount:
                    return this.VisitRowNumber((DbRowNumberExpression)exp);
                case DbExpressionType.Projection:
                    return this.VisitProjection((DbProjectionExpression)exp);
                case DbExpressionType.NamedValue:
                    return this.VisitNamedValue((DbNamedValueExpression)exp);
                case DbExpressionType.ClientJoin:
                    return this.VisitClientJoin((DbClientJoinExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }
        protected virtual Expression VisitTable(DbTableExpression table)
        {
            return table;
        }
        protected virtual Expression VisitColumn(DbColumnExpression column)
        {
            return column;
        }
        protected virtual Expression VisitSelect(DbSelectExpression select)
        {
            Expression from = this.VisitSource(select.From);
            Expression where = this.Visit(select.Where);
            ReadOnlyCollection<DbOrderByDeclaration> orderBy = this.VisitOrderBy(select.OrderBy);
            ReadOnlyCollection<Expression> groupBy = this.VisitExpressionList(select.GroupBy);
            Expression skip = this.Visit(select.Skip);
            Expression take = this.Visit(select.Take);
            ReadOnlyCollection<DbColumnDeclaration> columns = this.VisitColumnDeclarations(select.Columns);
            if (from != select.From
                || where != select.Where
                || orderBy != select.OrderBy
                || groupBy != select.GroupBy
                || take != select.Take
                || skip != select.Skip
                || columns != select.Columns
                )
            {
                return new DbSelectExpression(select.Alias, columns, from, where, orderBy, groupBy, select.IsDistinct, skip, take);
            }
            return select;
        }
        protected virtual Expression VisitJoin(DbJoinExpression join)
        {
            Expression left = this.VisitSource(join.Left);
            Expression right = this.VisitSource(join.Right);
            Expression condition = this.Visit(join.Condition);
            if (left != join.Left || right != join.Right || condition != join.Condition)
            {
                return new DbJoinExpression(join.Join, left, right, condition);
            }
            return join;
        }
        protected virtual Expression VisitOuterJoined(DbOuterJoinedExpression outer)
        {
            Expression test = this.Visit(outer.Test);
            Expression expression = this.Visit(outer.Expression);
            if (test != outer.Test || expression != outer.Expression)
            {
                return new DbOuterJoinedExpression(test, expression);
            }
            return outer;
        }
        protected virtual Expression VisitAggregate(DbAggregateExpression aggregate)
        {
            Expression arg = this.Visit(aggregate.Argument);
            if (arg != aggregate.Argument)
            {
                return new DbAggregateExpression(aggregate.Type, aggregate.AggregateType, arg, aggregate.IsDistinct);
            }
            return aggregate;
        }
        protected virtual Expression VisitIsNull(DbIsNullExpression isnull)
        {
            Expression expr = this.Visit(isnull.Expression);
            if (expr != isnull.Expression)
            {
                return new DbIsNullExpression(expr);
            }
            return isnull;
        }
        protected virtual Expression VisitBetween(DbBetweenExpression between)
        {
            Expression expr = this.Visit(between.Expression);
            Expression lower = this.Visit(between.Lower);
            Expression upper = this.Visit(between.Upper);
            if (expr != between.Expression || lower != between.Lower || upper != between.Upper)
            {
                return new DbBetweenExpression(expr, lower, upper);
            }
            return between;
        }
        protected virtual Expression VisitRowNumber(DbRowNumberExpression rowNumber)
        {
            var orderby = this.VisitOrderBy(rowNumber.OrderBy);
            if (orderby != rowNumber.OrderBy)
            {
                return new DbRowNumberExpression(orderby);
            }
            return rowNumber;
        }
        protected virtual Expression VisitNamedValue(DbNamedValueExpression value)
        {
            return value;
        }
        protected virtual Expression VisitSubquery(DbSubQueryExpression subquery)
        {
            switch ((DbExpressionType)subquery.NodeType)
            {
                case DbExpressionType.Scalar:
                    return this.VisitScalar((DbScalarExpression)subquery);
                case DbExpressionType.Exists:
                    return this.VisitExists((DbExistsExpression)subquery);
                case DbExpressionType.In:
                    return this.VisitIn((DbInExpression)subquery);
            }
            return subquery;
        }

        protected virtual Expression VisitScalar(DbScalarExpression scalar)
        {
            DbSelectExpression select = (DbSelectExpression)this.Visit(scalar.Select);
            if (select != scalar.Select)
            {
                return new DbScalarExpression(scalar.Type, select);
            }
            return scalar;
        }

        protected virtual Expression VisitExists(DbExistsExpression exists)
        {
            DbSelectExpression select = (DbSelectExpression)this.Visit(exists.Select);
            if (select != exists.Select)
            {
                return new DbExistsExpression(select);
            }
            return exists;
        }

        protected virtual Expression VisitIn(DbInExpression @in)
        {
            Expression expr = this.Visit(@in.Expression);
            if (@in.Select != null)
            {
                DbSelectExpression select = (DbSelectExpression)this.Visit(@in.Select);
                if (expr != @in.Expression || select != @in.Select)
                {
                    return new DbInExpression(expr, select);
                }
            }
            else
            {
                IEnumerable<Expression> values = this.VisitExpressionList(@in.Values);
                if (expr != @in.Expression || values != @in.Values)
                {
                    return new DbInExpression(expr, values);
                }
            }
            return @in;
        }

        protected virtual Expression VisitAggregateSubquery(DbAggregateSubQueryExpression aggregate)
        {
            Expression e = this.Visit(aggregate.AggregateAsSubQuery);
            System.Diagnostics.Debug.Assert(e is DbScalarExpression);
            DbScalarExpression subquery = (DbScalarExpression)e;
            if (subquery != aggregate.AggregateAsSubQuery)
            {
                return new DbAggregateSubQueryExpression(aggregate.GroupByAlias, aggregate.AggregateInGroupSelect, subquery);
            }
            return aggregate;
        }

        protected virtual Expression VisitSource(Expression source)
        {
            return this.Visit(source);
        }

        protected virtual Expression VisitProjection(DbProjectionExpression proj)
        {
            DbSelectExpression source = (DbSelectExpression)this.Visit(proj.Source);
            Expression projector = this.Visit(proj.Projector);
            if (source != proj.Source || projector != proj.Projector)
            {
                return new DbProjectionExpression(source, projector, proj.Aggregator);
            }
            return proj;
        }

        protected virtual Expression VisitClientJoin(DbClientJoinExpression join)
        {
            DbProjectionExpression projection = (DbProjectionExpression)this.Visit(join.Projection);
            var outerKey = this.VisitExpressionList(join.OuterKey);
            var innerKey = this.VisitExpressionList(join.InnerKey);
            if (projection != join.Projection || outerKey != join.OuterKey || innerKey != join.InnerKey)
            {
                return new DbClientJoinExpression(projection, outerKey, innerKey);
            }
            return join;
        }

        protected virtual ReadOnlyCollection<DbColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<DbColumnDeclaration> columns)
        {
            List<DbColumnDeclaration> alternate = null;
            for (int i = 0, n = columns.Count; i < n; i++)
            {
                DbColumnDeclaration column = columns[i];
                Expression e = this.Visit(column.Expression);
                if (alternate == null && e != column.Expression)
                {
                    alternate = columns.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(new DbColumnDeclaration(column.Name, e));
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return columns;
        }

        protected virtual ReadOnlyCollection<DbOrderByDeclaration> VisitOrderBy(ReadOnlyCollection<DbOrderByDeclaration> expressions)
        {
            if (expressions != null)
            {
                List<DbOrderByDeclaration> alternate = null;
                for (int i = 0, n = expressions.Count; i < n; i++)
                {
                    DbOrderByDeclaration expr = expressions[i];
                    Expression e = this.Visit(expr.Expression);
                    if (alternate == null && e != expr.Expression)
                    {
                        alternate = expressions.Take(i).ToList();
                    }
                    if (alternate != null)
                    {
                        alternate.Add(new DbOrderByDeclaration(expr.OrderByType, e));
                    }
                }
                if (alternate != null)
                {
                    return alternate.AsReadOnly();
                }
            }
            return expressions;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            if (original != null)
            {
                List<Expression> list = null;
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    Expression p = this.Visit(original[i]);
                    if (list != null)
                    {
                        list.Add(p);
                    }
                    else if (p != original[i])
                    {
                        list = new List<Expression>(n);
                        for (int j = 0; j < i; j++)
                        {
                            list.Add(original[j]);
                        }
                        list.Add(p);
                    }
                }
                if (list != null)
                {
                    return list.AsReadOnly();
                }
            }
            return original;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    public abstract partial class DbExpressionVisitor
        : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node.IsNull())
            {
                return node;
            }

            switch ((DbExpressionType)node.NodeType)
            {
                case DbExpressionType.Table:
                    return this.VisitExpression((DbTableExpression)node);
                case DbExpressionType.Column:
                    return this.VisitExpression((DbColumnExpression)node);
                case DbExpressionType.Select:
                    return this.VisitSelect((DbSelectExpression)node);
                case DbExpressionType.Join:
                    return this.VisitJoin((DbJoinExpression)node);
                case DbExpressionType.OuterJoined:
                    return this.VisitOuterJoined((DbOuterJoinedExpression)node);
                case DbExpressionType.Aggregate:
                    return this.VisitAggregate((DbAggregateExpression)node);
                case DbExpressionType.Scalar:
                case DbExpressionType.Exists:
                case DbExpressionType.In:
                case DbExpressionType.NotIn:
                case DbExpressionType.AggregateSubQuery:
                    return this.VisitAggregateSubQuery((DbAggregateSubQueryExpression)node);
                case DbExpressionType.IsNull:
                case DbExpressionType.IsNotNull:
                    return this.VisitNull((DbIsNullExpression)node);
                case DbExpressionType.Between:
                case DbExpressionType.NotBetween:
                    return this.VisitBetween((DbBetweenExpression)node);
                case DbExpressionType.RowCount:
                    return this.VisitRowNumber((DbRowNumberExpression)node);
                case DbExpressionType.Projection:
                    return this.VisitProjection((DbProjectionExpression)node);
                case DbExpressionType.NamedValue:
                    return this.VisitExpression((DbNamedValueExpression)node);
                case DbExpressionType.ClientJoin:
                    return this.VisitClientJoin((DbClientJoinExpression)node);
                default:
                    return base.Visit(node);
            }
        }

        protected virtual Expression VisitClientJoin(DbClientJoinExpression join)
        {
            if (join is null)
            {
                return join;
            }

            DbProjectionExpression projection = (DbProjectionExpression)this.Visit(join.Projection);

            var outerKey = this.VisitExpressionList(join.OuterKey);
            var innerKey = this.VisitExpressionList(join.InnerKey);

            if (projection != join.Projection || outerKey != join.OuterKey || innerKey != join.InnerKey)
            {
                return new DbClientJoinExpression(projection, outerKey, innerKey);
            }

            return join;
        }

        protected virtual Expression VisitProjection(DbProjectionExpression projection)
        {
            if(projection is null)
            {
                return projection;
            }

            DbSelectExpression source = (DbSelectExpression)this.Visit(projection.Source);
            Expression projector = this.Visit(projection.Projector);
            if (source != projection.Source || projector != projection.Projector)
            {
                return new DbProjectionExpression(source, projector, projection.Aggregator);
            }
            return projection;
        }

        protected virtual Expression VisitRowNumber(DbRowNumberExpression rowNumber)
        {
            if(rowNumber is null)
            {
                return rowNumber;
            }

            var orderby = this.VisitOrderBy(rowNumber.OrderBy);

            if (orderby != rowNumber.OrderBy)
            {
                return new DbRowNumberExpression(orderby);
            }

            return rowNumber;
        }

        protected virtual Expression VisitBetween(DbBetweenExpression between)
        {
            if (between is null)
            {
                return between;
            }

            Expression expr = this.Visit(between.Expression);
            Expression lower = this.Visit(between.Lower);
            Expression upper = this.Visit(between.Upper);

            if (expr != between.Expression || lower != between.Lower || upper != between.Upper)
            {
                switch ((DbExpressionType)between.NodeType)
                {
                    case DbExpressionType.Between:
                        return new DbBetweenExpression(expr, lower, upper);
                    case DbExpressionType.NotBetween:
                        return new DbNotBetweenExpression(expr, lower, upper);
                }
            }

            return between;
        }

        protected virtual Expression VisitAggregateSubQuery(DbAggregateSubQueryExpression aggregate)
        {
            if (aggregate is null)
            {
                return aggregate;
            }

            Expression e = this.Visit(aggregate.AggregateAsSubQuery);

            switch((DbExpressionType)e.NodeType)
            {
                case DbExpressionType.Scalar:
                    {
                        DbScalarExpression subQuery = (DbScalarExpression)e;

                        if (subQuery != aggregate.AggregateAsSubQuery)
                        {
                            return new DbAggregateSubQueryExpression(aggregate.GroupByAlias, aggregate.AggregateInGroupSelect, subQuery);
                        }
                    }
                    break;
            }
            
            return aggregate;
        }

        protected virtual Expression VisitSubquery(DbSubQueryExpression subquery)
        {
            if(subquery is null)
            {
                return subquery;
            }

            switch ((DbExpressionType)subquery.NodeType)
            {
                case DbExpressionType.Scalar:
                    return this.VisitScalar((DbScalarExpression)subquery);
                case DbExpressionType.Exists:
                    return this.VisitExists((DbExistsExpression)subquery);
                case DbExpressionType.In:
                case DbExpressionType.NotIn:
                    return this.VisitIn((DbInExpression)subquery);
            }
            return subquery;
        }

        protected virtual Expression VisitScalar(DbScalarExpression scalar)
        {
            if (scalar is null)
            {
                return scalar;
            }

            DbSelectExpression select = (DbSelectExpression)this.Visit(scalar.Select);
            if (select != scalar.Select)
            {
                return new DbScalarExpression(scalar.Type, select);
            }
            return scalar;
        }

        protected virtual Expression VisitExists(DbExistsExpression exists)
        {
            if (exists is null)
            {
                return exists;
            }

            DbSelectExpression select = (DbSelectExpression)this.Visit(exists.Select);
            if (select != exists.Select)
            {
                return new DbExistsExpression(select);
            }
            return exists;
        }

        protected virtual Expression VisitIn(DbInExpression inExp)
        {
            if (inExp is null)
            {
                return inExp;
            }

            Expression expr = this.Visit(inExp.Expression);

            if (inExp.Select != null)
            {
                DbSelectExpression select = (DbSelectExpression)this.Visit(inExp.Select);
                if (expr != inExp.Expression || select != inExp.Select)
                {
                    switch ((DbExpressionType)inExp.NodeType)
                    {
                        case DbExpressionType.In:
                            return new DbInExpression(expr, select);
                        case DbExpressionType.NotIn:
                            return new DbNotInExpression(expr, select);
                    }
                }
            }
            else
            {
                IEnumerable<Expression> values = this.VisitExpressionList(inExp.Values);
                if (expr != inExp.Expression || values != inExp.Values)
                {
                    switch ((DbExpressionType)inExp.NodeType)
                    {
                        case DbExpressionType.In:
                            return new DbInExpression(expr, values);
                        case DbExpressionType.NotIn:
                            return new DbNotInExpression(expr, values);
                    }
                }
            }
            return inExp;
        }

        protected virtual Expression VisitNull(DbIsNullExpression isnull)
        {
            if(isnull is null)
            {
                return isnull;
            }

            Expression expr = this.Visit(isnull.Expression);

            if (expr != isnull.Expression)
            {
                switch ((DbExpressionType)isnull.NodeType)
                {
                    case DbExpressionType.IsNull:
                        return new DbIsNullExpression(expr);
                    case DbExpressionType.IsNotNull:
                        return new DbIsNotNullExpression(expr);
                }
            }

            return isnull;
        }

        protected virtual Expression VisitAggregate(DbAggregateExpression aggregate)
        {
            if (aggregate is null)
            {
                return aggregate;
            }

            Expression arg = this.Visit(aggregate.Argument);
            if (arg != aggregate.Argument)
            {
                return new DbAggregateExpression(aggregate.Type, aggregate.AggregateType, arg, aggregate.IsDistinct);
            }
            return aggregate;
        }

        protected virtual Expression VisitOuterJoined(DbOuterJoinedExpression outer)
        {
            if (outer is null)
            {
                return outer;
            }

            Expression test = this.Visit(outer.Test);
            Expression expression = this.Visit(outer.Expression);
            if (test != outer.Test || expression != outer.Expression)
            {
                return new DbOuterJoinedExpression(test, expression);
            }
            return outer;
        }

        protected virtual Expression VisitJoin(DbJoinExpression join)
        {
            if(join is null)
            {
                return join;
            }

            Expression left = this.VisitSource(join.Left);
            Expression right = this.VisitSource(join.Right);
            Expression condition = this.Visit(join.Condition);
            if (left != join.Left || right != join.Right || condition != join.Condition)
            {
                return new DbJoinExpression(join.Join, left, right, condition);
            }
            return join;
        }

        protected virtual Expression VisitSelect(DbSelectExpression selectExp)
        {
            if (selectExp.IsNull())
            {
                return selectExp;
            }

            Expression from = this.VisitSource(selectExp.From);
            Expression where = this.Visit(selectExp.Where);
            ReadOnlyCollection<DbOrderByDeclaration> orderBy = this.VisitOrderBy(selectExp.OrderBy);
            ReadOnlyCollection<Expression> groupBy = this.VisitExpressionList(selectExp.GroupBy);
            Expression skip = this.Visit(selectExp.Skip);
            Expression take = this.Visit(selectExp.Take);
            ReadOnlyCollection<DbColumnDeclaration> columns = this.VisitColumnDeclarations(selectExp.Columns);
            if (from != selectExp.From
                || where != selectExp.Where
                || orderBy != selectExp.OrderBy
                || groupBy != selectExp.GroupBy
                || take != selectExp.Take
                || skip != selectExp.Skip
                || columns != selectExp.Columns
                )
            {
                return new DbSelectExpression(selectExp.Alias, columns, from, where, orderBy, groupBy, selectExp.IsDistinct, skip, take);
            }
            return selectExp;
        }

        protected virtual ReadOnlyCollection<DbColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<DbColumnDeclaration> columns)
        {
            if (columns is null)
            {
                return columns;
            }

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

        protected virtual Expression VisitSource(Expression source)
        {
            return this.Visit(source);
        }

        protected virtual DbExpression VisitExpression(DbExpression expression)
            //where TExpression : DbExpression
        {
            return expression;
        }
    }
}
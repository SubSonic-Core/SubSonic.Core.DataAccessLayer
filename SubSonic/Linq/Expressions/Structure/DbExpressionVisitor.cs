using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Infrastructure;

    public abstract class DbExpressionVisitor
        : ExpressionVisitor
    {
        protected static readonly char[] splitters = new char[] { '\n', '\r' };
        protected static readonly char[] special = new char[] { '\n', '\t', '\\' };

        protected internal virtual Expression VisitClientJoin(DbClientJoinExpression join)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitDelete(DbDeleteExpression delete)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitProjection(DbProjectionExpression projection)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitRowNumber(DbRowNumberExpression rowNumber)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitBetween(DbBetweenExpression between)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitAggregateSubQuery(DbAggregateSubQueryExpression aggregate)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitScalar(DbScalarExpression scalar)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitIn(DbInExpression inExp)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitInsert(DbInsertExpression insert)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitUpdate(DbUpdateExpression update)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitNull(DbIsNullExpression isnull)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitAggregate(DbAggregateExpression aggregate)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitOuterJoined(DbOuterJoinedExpression outer)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitJoin(DbJoinExpression join, bool cte = false)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitSelect(DbExpression expression)
        {
            throw Error.NotImplemented();
        }

        protected virtual IReadOnlyCollection<DbColumnDeclaration> VisitColumnDeclarations(IReadOnlyCollection<DbColumnDeclaration> columns)
        {
            throw Error.NotImplemented();
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            throw Error.NotImplemented();
        }

        protected virtual ReadOnlyCollection<DbOrderByDeclaration> VisitOrderBy(ReadOnlyCollection<DbOrderByDeclaration> expressions)
        {
            throw Error.NotImplemented();
        }

        protected virtual Expression VisitSource(Expression source, bool cte = false)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitFunction(DbFunctionExpression dbFunction)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual DbExpression VisitExpression(DbExpression expression)
        {
            throw Error.NotImplemented();
        }

        protected internal virtual Expression VisitWhere(DbWhereExpression where)
        {
            throw Error.NotImplemented();
        }
    }
}

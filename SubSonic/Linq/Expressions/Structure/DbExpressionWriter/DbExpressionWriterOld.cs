using SubSonic;
using SubSonic.Linq.Expressions.Alias;
using SubSonic.Linq.Microsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Translation;

    public class DbExpressionWriterOld
        : Microsoft.ExpressionWriter
    {
        Dictionary<Table, int> aliasMap = new Dictionary<Table, int>();

        protected DbExpressionWriterOld(TextWriter writer)
            : base(writer)
        {
        }

        public override Expression Visit(Expression exp)
        {
            if (exp == null)
                return null;

            switch ((DbExpressionType)exp.NodeType)
            {
                case DbExpressionType.Projection:
                    return VisitProjection((DbProjectionExpression)exp);
                case DbExpressionType.ClientJoin:
                    return VisitClientJoin((DbClientJoinExpression)exp);
                case DbExpressionType.Select:
                    return VisitSelect((DbSelectExpression)exp);
                case DbExpressionType.OuterJoined:
                    return VisitOuterJoined((DbOuterJoinedExpression)exp);
                case DbExpressionType.Column:
                    return VisitColumn((DbColumnExpression)exp);
                default:
                    if (exp is DbExpression)
                    {
                        Write(TSqlFormatter.Format(exp));
                        return exp;
                    }
                    else
                    {
                        return base.Visit(exp);
                    }
            }
        }

        protected void AddAlias(Table alias)
        {
            if (!aliasMap.ContainsKey(alias))
            {
                aliasMap.Add(alias, aliasMap.Count);
            }
        }

        protected virtual Expression VisitProjection(DbProjectionExpression projection)
        {
            AddAlias(projection.Source.Alias);
            Write("Project(");
            WriteLine(Indentation.Inner);
            Write("@\"");
            Visit(projection.Source);
            Write("\",");
            WriteLine(Indentation.Same);
            Visit(projection.Projector);
            Write(",");
            WriteLine(Indentation.Same);
            Visit(projection.Aggregator);
            WriteLine(Indentation.Outer);
            Write(")");
            return projection;
        }

        protected virtual Expression VisitClientJoin(DbClientJoinExpression join)
        {
            this.AddAlias(join.Projection.Source.Alias);
            Write("ClientJoin(");
            WriteLine(Indentation.Inner);
            Write("OuterKey(");
            this.VisitExpressionList(join.OuterKey);
            Write("),");
            WriteLine(Indentation.Same);
            Write("InnerKey(");
            this.VisitExpressionList(join.InnerKey);
            Write("),");
            WriteLine(Indentation.Same);
            this.Visit(join.Projection);
            WriteLine(Indentation.Outer);
            Write(")");
            return join;
        }

        protected virtual Expression VisitOuterJoined(DbOuterJoinedExpression outer)
        {
            Write("Outer(");
            WriteLine(Indentation.Inner);
            Visit(outer.Test);
            Write(", ");
            WriteLine(Indentation.Same);
            Visit(outer.Expression);
            WriteLine(Indentation.Outer);
            Write(")");
            return outer;
        }

        protected virtual Expression VisitSelect(DbSelectExpression select)
        {
            Write(select.QueryText);
            return select;
        }

        protected virtual Expression VisitColumn(DbColumnExpression column)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            int iAlias;
            string aliasName =
                aliasMap.TryGetValue(column.Alias, out iAlias)
                ? "A" + iAlias
                : "A?";

            Write(aliasName);
            Write(".");
            Write("Column(\"");
            Write(column.Name);
            Write("\")");
            return column;
        }
    }
}

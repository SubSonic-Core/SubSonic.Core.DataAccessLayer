using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Alias;

    public partial class DbExpressionWriter
    {
        Dictionary<TableAlias, int> aliasMap = new Dictionary<TableAlias, int>();

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

        protected void AddAlias(TableAlias alias)
        {
            if (!aliasMap.ContainsKey(alias))
            {
                aliasMap.Add(alias, aliasMap.Count);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected internal override Expression VisitProjection(DbProjectionExpression projection)
        {
            if (projection != null)
            {
                AddAlias(projection.Source.Table);
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
            }

            return projection;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected internal override Expression VisitClientJoin(DbClientJoinExpression join)
        {
            if (join != null)
            {
                this.AddAlias(join.Projection.Source.Table);
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
            }

            return join;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected internal override Expression VisitOuterJoined(DbOuterJoinedExpression outer)
        {
            if (outer != null)
            {
                Write("Outer(");
                WriteLine(Indentation.Inner);
                Visit(outer.Test);
                Write(", ");
                WriteLine(Indentation.Same);
                Visit(outer.Expression);
                WriteLine(Indentation.Outer);
                Write(")");
            }

            return outer;
        }

        protected internal override Expression VisitSelect(DbSelectExpression select)
        {
            if (select != null)
            {
                Write(select.QueryText);
            }

            return select;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
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
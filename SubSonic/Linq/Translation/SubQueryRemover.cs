using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Linq.Translation
{
    using Expressions;
    using Expressions.Alias;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Removes one or more DbSelectExpression's by rewriting the expression tree to not include them, promoting
    /// their from clause expressions and rewriting any column expressions that may have referenced them to now
    /// reference the underlying data directly.
    /// </summary>
    public class SubQueryRemover : DbExpressionVisitorOld
    {
        HashSet<DbSelectExpression> selectsToRemove;
        Dictionary<Table, Dictionary<string, Expression>> map;

        private SubQueryRemover(IEnumerable<DbSelectExpression> selectsToRemove)
        {
            this.selectsToRemove = new HashSet<DbSelectExpression>(selectsToRemove);
            this.map = this.selectsToRemove.ToDictionary(d => d.Alias, d => d.Columns.ToDictionary(d2 => d2.Name, d2 => d2.Expression));
        }

        public static DbSelectExpression Remove(DbSelectExpression outerSelect, params DbSelectExpression[] selectsToRemove)
        {
            return Remove(outerSelect, (IEnumerable<DbSelectExpression>)selectsToRemove);
        }

        public static DbSelectExpression Remove(DbSelectExpression outerSelect, IEnumerable<DbSelectExpression> selectsToRemove)
        {
            return (DbSelectExpression)new SubQueryRemover(selectsToRemove).Visit(outerSelect);
        }

        public static DbProjectionExpression Remove(DbProjectionExpression projection, params DbSelectExpression[] selectsToRemove)
        {
            return Remove(projection, (IEnumerable<DbSelectExpression>)selectsToRemove);
        }

        public static DbProjectionExpression Remove(DbProjectionExpression projection, IEnumerable<DbSelectExpression> selectsToRemove)
        {
            return (DbProjectionExpression)new SubQueryRemover(selectsToRemove).Visit(projection);
        }

        protected override Expression VisitSelect(DbSelectExpression select)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (this.selectsToRemove.Contains(select))
            {
                return this.Visit(select.From);
            }
            else
            {
                return base.VisitSelect(select);
            }
        }

        protected override Expression VisitColumn(DbColumnExpression column)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            Dictionary<string, Expression> nameMap;
            if (this.map.TryGetValue(column.Alias, out nameMap))
            {
                Expression expr;
                if (nameMap.TryGetValue(column.Name, out expr))
                {
                    return this.Visit(expr);
                }
                throw new Exception("Reference to undefined column");
            }
            return column;
        }
    }
}

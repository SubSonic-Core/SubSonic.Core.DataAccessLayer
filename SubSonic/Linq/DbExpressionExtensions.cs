using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq
{
    using Expressions;

    public static partial class SubSonicExtensions
    {
        public static DbSelectExpression SetColumns(this DbSelectExpression select, IEnumerable<DbColumnDeclaration> columns)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (columns is null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            return new DbSelectExpression(select.QueryObject, select.From, columns.OrderBy(c => c.Order), select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
        }

        public static DbSelectExpression AddColumn(this DbSelectExpression select, DbColumnDeclaration column)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            List<DbColumnDeclaration> columns = new List<DbColumnDeclaration>(select.Columns);
            columns.Add(column);
            return select.SetColumns(columns);
        }

        public static DbSelectExpression RemoveColumn(this DbSelectExpression select, DbColumnDeclaration column)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            List<DbColumnDeclaration> columns = new List<DbColumnDeclaration>(select.Columns);
            columns.Remove(column);
            return select.SetColumns(columns);
        }

        public static string GetAvailableColumnName(this DbSelectExpression select, string baseName)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (string.IsNullOrEmpty(baseName))
            {
                throw new ArgumentException("", nameof(baseName));
            }

            string name = baseName;
            int n = 0;
            while (!IsUniqueName(select, name))
            {
                name = baseName + (n++);
            }
            return name;
        }

        private static bool IsUniqueName(DbSelectExpression select, string name)
        {
            foreach (var col in select.Columns)
            {
                if (col.PropertyName == name)
                {
                    return false;
                }
            }
            return true;
        }

        public static DbProjectionExpression AddOuterJoinTest(this DbProjectionExpression projection)
        {
            if (projection is null)
            {
                throw new ArgumentNullException(nameof(projection));
            }

            string colName = projection.Source.GetAvailableColumnName("Test");
            DbSelectExpression newSource = projection.Source.AddColumn(new DbColumnDeclaration(colName, projection.Source.Columns.Count, Expression.Constant(1, typeof(int?))));
            Expression newProjector =
                new DbOuterJoinedExpression(
                    new DbColumnExpression(typeof(int?), newSource.Table, colName),
                    projection.Projector
                    );
            return new DbProjectionExpression(newSource, newProjector, projection.Aggregator);
        }

        public static DbSelectExpression SetDistinct(this DbSelectExpression select, bool isDistinct)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (select.IsDistinct != isDistinct)
            {
                return new DbSelectExpression(select.QueryObject, select.From, select.Columns, select.Where, select.OrderBy, select.GroupBy, isDistinct, select.Skip, select.Take);
            }
            return select;
        }

        public static DbSelectExpression SetWhere(this DbSelectExpression select, Expression where)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (where is null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            if (where != select.Where)
            {
                return new DbSelectExpression(select.QueryObject, select.From, select.Columns, where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
            }
            return select;
        }

        public static DbSelectExpression SetOrderBy(this DbSelectExpression select, IEnumerable<DbOrderByDeclaration> orderBy)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (orderBy is null)
            {
                throw new ArgumentNullException(nameof(orderBy));
            }

            return new DbSelectExpression(select.QueryObject, select.From, select.Columns, select.Where, orderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
        }

        public static DbSelectExpression AddDbOrderByDeclaration(this DbSelectExpression select, DbOrderByDeclaration ordering)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (ordering is null)
            {
                throw new ArgumentNullException(nameof(ordering));
            }

            List<DbOrderByDeclaration> orderby = new List<DbOrderByDeclaration>();
            if (select.OrderBy != null)
                orderby.AddRange(select.OrderBy);
            orderby.Add(ordering);
            return select.SetOrderBy(orderby);
        }

        public static DbSelectExpression RemoveDbOrderByDeclaration(this DbSelectExpression select, DbOrderByDeclaration ordering)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (ordering is null)
            {
                throw new ArgumentNullException(nameof(ordering));
            }

            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                List<DbOrderByDeclaration> orderby = new List<DbOrderByDeclaration>(select.OrderBy);
                orderby.Remove(ordering);
                return select.SetOrderBy(orderby);
            }
            return select;
        }

        public static DbSelectExpression SetGroupBy(this DbSelectExpression select, IEnumerable<Expression> groupBy)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (groupBy is null)
            {
                throw new ArgumentNullException(nameof(groupBy));
            }

            return new DbSelectExpression(select.QueryObject, select.From, select.Columns, select.Where, select.OrderBy, groupBy, select.IsDistinct, select.Skip, select.Take);
        }

        public static DbSelectExpression AddGroupExpression(this DbSelectExpression select, Expression expression)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            List<Expression> groupby = new List<Expression>();
            if (select.GroupBy != null)
                groupby.AddRange(select.GroupBy);
            groupby.Add(expression);
            return select.SetGroupBy(groupby);
        }

        public static DbSelectExpression RemoveGroupExpression(this DbSelectExpression select, Expression expression)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (select.GroupBy != null && select.GroupBy.Count > 0)
            {
                List<Expression> groupby = new List<Expression>(select.GroupBy);
                groupby.Remove(expression);
                return select.SetGroupBy(groupby);
            }
            return select;
        }

        public static DbSelectExpression SetSkip(this DbSelectExpression select, Expression skip)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (skip is null)
            {
                throw new ArgumentNullException(nameof(skip));
            }

            if (skip != select.Skip)
            {

                return new DbSelectExpression(select.QueryObject, select.From, select.Columns, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, skip, select.Take);
            }
            return select;
        }

        public static DbSelectExpression SetTake(this DbSelectExpression select, Expression take)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (take is null)
            {
                throw new ArgumentNullException(nameof(take));
            }

            if (take != select.Take)
            {
                return new DbSelectExpression(select.QueryObject, select.From, select.Columns, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, take);
            }
            return select;
        }
    }
}

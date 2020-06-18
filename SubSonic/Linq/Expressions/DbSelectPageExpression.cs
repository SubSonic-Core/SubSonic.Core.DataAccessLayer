using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Factory;
using SubSonic.Linq.Expressions.Structure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// a custom expression used to generate a SELECT statement with CTE using OFFSET and FETCH
    /// </summary>
    public class DbSelectPageExpression
        : DbConstantExpression
    {
        private DbSelectExpression _primaryKeySelect;

        protected internal DbSelectPageExpression(DbSelectExpression select, int pageNumber, int pageSize)
            : base(
                  select.IsNullThrowArgumentNull(nameof(select)).QueryObject,
                  select.IsNullThrowArgumentNull(nameof(select)).Type,
                  select.IsNullThrowArgumentNull(nameof(select)).Alias)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            PageCte = (DbTableExpression)DbTable(select.From.Model.Table, "page");
            Select = (DbSelectExpression)DbSelect(select, (DbJoinExpression)DbJoin(JoinType.InnerJoin, select.From, PageCte));
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public DbSelectExpression Select { get; }

        public DbTableExpression PageCte { get; }

        public DbSelectExpression PrimaryKeySelect =>
            _primaryKeySelect ?? (_primaryKeySelect = (DbSelectExpression)DbSelect(
                    Select.QueryObject,
                    Select.Type,
                    Select.From,
                    Select.From.Columns.Where(column => column.Property.IsPrimaryKey),
                    Select.Where,
                    Select.OrderBy,
                    true
                ));

        public int PageNumber { get; set; }

        public int PageSize { get; }

        public IEnumerable<DbParameter> Parameters
        {
            get
            {
                DbProviderFactory provider = DbContext.ServiceProvider.GetService<DbProviderFactory>();

                if (provider is SubSonicDbProvider subsonic)
                {
                    return new[]
                    {
                        subsonic.CreateParameter($"@{nameof(PageNumber)}", PageNumber),
                        subsonic.CreateParameter($"@{nameof(PageSize)}", PageSize)
                    };
                }

                throw new NotSupportedException();
            }
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitSelect(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbPagedSelect(DbExpression expression, int pageNumber, int pageSize)
        {
            if (expression is DbSelectExpression select)
            {
                if (select.OrderBy is null || select.OrderBy.Count == 0)
                {
                    select = (DbSelectExpression)DbSelect(select.QueryObject, select.Type, select.From, select.Columns, select.Where,
                        select.From.Columns
                            .Where((column) =>
                                column.Property.IsPrimaryKey)
                            .Select((column) => new DbOrderByDeclaration(OrderByType.Ascending, column.Expression)));
                }

                return new DbSelectPageExpression(select, pageNumber, pageSize);
            }

            throw new NotSupportedException();
        }
    }
}

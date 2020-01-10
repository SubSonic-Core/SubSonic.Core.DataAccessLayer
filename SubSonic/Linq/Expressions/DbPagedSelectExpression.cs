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
    public class DbPagedSelectExpression
        : DbConstantExpression
    {
        protected internal DbPagedSelectExpression(DbSelectExpression select, int pageNumber, int pageSize)
            : base(
                  select.IsNullThrowArgumentNull(nameof(select)).QueryObject,
                  select.IsNullThrowArgumentNull(nameof(select)).Table)
        {
            Select = select ?? throw new ArgumentNullException(nameof(select));
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public DbSelectExpression Select { get; }

        public DbSelectExpression SelectPrimaryKey
        {
            get
            {
                return (DbSelectExpression)DbExpression.DbSelect(
                    Select.QueryObject,
                    Select.From,
                    Select.From.Columns.Where(column => column.Property.IsPrimaryKey),
                    Select.Where,
                    Select.OrderBy);
            }
        }

        public int PageNumber { get; }

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
                        subsonic.CreateParameter(nameof(PageNumber), PageNumber),
                        subsonic.CreateParameter(nameof(PageSize), PageSize)
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
                return new DbPagedSelectExpression(select, pageNumber, pageSize);
            }

            throw new NotSupportedException();
        }
    }
}

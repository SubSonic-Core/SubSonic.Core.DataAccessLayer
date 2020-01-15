using SubSonic.Linq;
using SubSonic.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbPagedQuery
        : DbQuery
        , IDbPagedQuery
    {
        public DbPagedQuery(DbSelectExpression select, int size)
            : this((DbSelectPagedExpression)DbExpression.DbPagedSelect(select, 1, size))
        {

        }

        public DbPagedQuery(DbSelectPagedExpression select)
            : base(CommandBehavior.Default)
        {
            PageSize = select.PageSize;
            PageNumber = select.PageNumber;

            if (select.Select.From.Columns.Count(x => x.Property.IsPrimaryKey) == 1)
            {
                SelectRecordCount = DbExpression.DbSelectAggregate(select.PrimaryKeySelect, new[] { DbExpression.DbAggregate(typeof(int), AggregateType.Count, select.Select.From.Columns.Single(x => x.Property.IsPrimaryKey).Expression) });
            }
            else
            {
                SelectRecordCount = DbExpression.DbSelectAggregate(select.PrimaryKeySelect, new[] { DbExpression.DbAggregate(typeof(int), AggregateType.Count, null) });
            }
                        
            SelectPaged = select;
        }

        public DbExpression SelectRecordCount { get; }

        public DbExpression SelectPaged { get; }

        public int Count { get; private set; }

        public int PageSize { get; }

        public int PageNumber { get; set; }

        public int PageCount => (int)Math.Ceiling((decimal)(Count / PageSize));

        public override string Sql => ToString();

        public IEnumerable<TEntity> GetRecordsForPage<TEntity>(int number)
        {
            PageNumber = number;

            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return String.Join("", SelectRecordCount, "\r\n", SelectPaged);
        }
    }
}

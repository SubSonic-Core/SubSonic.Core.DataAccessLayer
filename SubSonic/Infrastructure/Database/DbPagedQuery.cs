using SubSonic.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;

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
            if (select.Select.From.Columns.Count(x => x.Property.IsPrimaryKey) == 1)
            {
                SelectRecordCount = DbExpression.DbSelectAggregate(select.PrimaryKeySelect, new[] { 
                    DbExpression.DbColumnAggregate(DbExpression.DbAggregate(typeof(int), AggregateType.Count, select.Select.From.Columns.Single(x => x.Property.IsPrimaryKey).Expression), nameof(RecordCount)) });
            }
            else
            {
                SelectRecordCount = DbExpression.DbSelectAggregate(select.PrimaryKeySelect, new[] { 
                    DbExpression.DbColumnAggregate(DbExpression.DbAggregate(typeof(int), AggregateType.Count, null), nameof(RecordCount)) });
            }
                        
            SelectPaged = select;
        }

        public DbExpression SelectRecordCount { get; }

        public DbSelectPagedExpression SelectPaged { get; }

        public int RecordCount { get; set; }

        public int PageSize => SelectPaged.PageSize;

        public int PageNumber
        {
            get => SelectPaged.PageNumber;
            set => SelectPaged.PageNumber = value;
        }

        public int PageCount => (int)Math.Ceiling((decimal)RecordCount / PageSize);

        public override string Sql => ToString();

        public override IReadOnlyCollection<DbParameter> Parameters => GetParameters();

        public IReadOnlyCollection<DbParameter> GetParameters()
        {
            IEnumerable<DbParameter> parameters = SelectPaged.Parameters;

            if (SelectPaged.Select.Where is DbWhereExpression where)
            {
                parameters = parameters.Union(where.Parameters);    
            }

            return new ReadOnlyCollection<DbParameter>(parameters.ToList());
        }

        public IDbPageCollection<TEntity> ToPagedCollection<TEntity>()
        {
            return new DbPageCollection<TEntity>(this);
        }

        public override string ToString()
        {
            return String.Join($";{Environment.NewLine}", SelectRecordCount, SelectPaged);
        }
    }
}

using SubSonic.Linq.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbPageCollection<TEntity>
        : IDbPageCollection<TEntity>
    {
        private readonly DbPagedQuery pagedQuery;

        public DbPageCollection(DbPagedQuery query)
        {
            this.pagedQuery = query;
        }

        public int RecordCount
        {
            get => pagedQuery.RecordCount;
            set => pagedQuery.RecordCount = value;
        }

        public int PageSize => pagedQuery.PageSize;

        public int PageCount => pagedQuery.PageCount;

        public int PageNumber
        {
            get => pagedQuery.PageNumber;
            set => pagedQuery.PageNumber = value;
        }

        public IDbPagesCollection<TEntity> GetPages()
        {
            return new DbPagesCollection<TEntity>(this);
        }

        public IEnumerable<TEntity> GetRecordsForPage(int page)
        {
            pagedQuery.PageNumber = page;

            return GetRecordsForPage();
        }

        public IEnumerable<TEntity> GetRecordsForPage()
        {
            using (DataSet data = new DataSet())
            {
                if (DbContext.Current.Database.ExecuteAdapter(pagedQuery, data))
                {
                    foreach(DataTable table in data.Tables)
                    {
                        if (table.Columns.Count == 1 && table.Columns[0].ColumnName == nameof(RecordCount).ToUpper(CultureInfo.CurrentCulture))
                        {
                            RecordCount = (int)table.Rows[0][0];

                            continue;
                        }

                        return table.CreateDataReader().Map<TEntity>();
                    }
                }

                return Array.Empty<TEntity>();
            }           
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return GetRecordsForPage(PageNumber == 0 ? 1 : PageNumber).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

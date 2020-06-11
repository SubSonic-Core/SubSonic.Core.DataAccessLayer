using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SubSonic.Extensions.Test.Data.Builders
{
    public class DataTableBuilder : IDisposable
    {
        private DataTable _table;
        public DataTableBuilder(string tableName = null)
        {
            _table = new DataTable(tableName);
        }

        public DataTableBuilder(DataTable table)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public DataTable DataTable
        {
            get
            {
                return _table;
            }
        }

        public DataRow CreateRow()
        {
            return _table.NewRow();
        }

        public DataTableBuilder AddColumn(string name, Type type)
        {
            _table.Columns.Add(name, type.GetUnderlyingType());
            return this;
        }

        public DataTableBuilder AddRow(params object[] data)
        {
            _table.Rows.Add(data);
            return this;
        }

        public void AddRow(DataRow row)
        {
            _table.Rows.Add(row);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _table.Dispose();
                    _table = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DataTableBuilder()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
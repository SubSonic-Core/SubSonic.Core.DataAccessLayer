using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    using SqlGenerator;
    using SubSonic.Infrastructure.Schema;
    using System.IO;

    public class SqlQueryProvider
        : ISqlQueryProvider
        , IDisposable
    {
        private readonly StringBuilder sql;
        private TextWriter sqlWriter;

        protected SqlQueryProvider(ISqlFragment sqlFragment)
        {
            this.sqlFragment = sqlFragment ?? throw new ArgumentNullException(nameof(sqlFragment));
            this.sql = new StringBuilder();
            this.sqlWriter = new StringWriter(sql);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "really this should throw if not overridden")]
        public virtual string ClientName => throw new NotImplementedException();

        public ISqlFragment sqlFragment { get; }

        public void WriteSqlSegment(string segment, bool debug = false)
        {
            if (debug)
            {
                sqlWriter.WriteLine(segment);
            }
            else
            {
                sqlWriter.Write(segment);
            }
        }

        public override string ToString()
        {
            return sql.ToString();
        }

        public virtual ISqlGenerator BuildDeleteStatement()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildInsertStatement()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildPagedSelectStatement()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildSelectStatement()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildUpdateStatement()
        {
            throw new NotImplementedException();
        }

        public virtual IDbEntityProperty FindColumn(string columnName)
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateCommandLine()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateConstraints()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateFromList()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateGroupBy()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateJoins()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateOrderBy()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GetPagingSqlWrapper()
        {
            throw new NotImplementedException();
        }

        public virtual List<string> GetSelectColumns()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sqlWriter.Dispose();
                    sqlWriter = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SqlQueryProvider()
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

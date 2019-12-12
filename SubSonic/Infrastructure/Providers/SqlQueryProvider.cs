using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Schema;
    using SqlGenerator;

    public class SqlQueryProvider
        : ISqlQueryProvider
        , IDisposable
    {
        private readonly StringBuilder sql;
        private TextWriter sqlWriter;

        private const string PAGING_SQL =
@"
DECLARE @Page int;
DECLARE @PageSize int;

SELECT @Page = {4}, @PageSize = {5};

SET NOCOUNT ON;

-- create a temp table to hold order ids
DECLARE @TempTable TABLE (IndexId int identity, _keyID {6});

-- insert the table ids and row numbers into the memory table
INSERT INTO @TempTable
(
	_keyID
)
SELECT 
	{0}
	{1}
	{2}
-- select only those rows belonging to the proper page
	{3}
INNER JOIN @TempTable t ON {0} = t._keyID
WHERE t.IndexId BETWEEN ((@Page - 1) * @PageSize + 1) AND (@Page * @PageSize);";

        public static ISqlContext CreateContext<TSqlFragment, TSqlMethods>()
        {
            throw new NotImplementedException();
        }

        protected SqlQueryProvider(ISqlContext sqlContext)
        {
            this.SqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
            this.sql = new StringBuilder();
            this.sqlWriter = new StringWriter(sql);
        }

        public IDbEntityModel EntityModel { get; set; }

        public virtual string ClientName => string.Empty;

        public ISqlContext SqlContext { get; }

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

        public virtual string GetPagingSqlWrapper()
        {
            return PAGING_SQL;
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

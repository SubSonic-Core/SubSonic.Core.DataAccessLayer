using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class AutomaticConnectionScope
        : IDisposable
    {
        private readonly DbDatabase dbDatabase;
        private readonly DbConnection dbConnection;

        private bool isUsingSharedConnection;

        public AutomaticConnectionScope(DbDatabase dbDatabase)
        {
            this.dbDatabase = dbDatabase ?? throw new ArgumentNullException(nameof(dbDatabase));

            if (dbDatabase.CurrentSharedConnection.IsNotNull())
            {
                dbConnection = dbDatabase.CurrentSharedConnection;
                isUsingSharedConnection = true;
            }
            else
            {
                dbConnection = dbDatabase.CreateConnection();
            }
        }

        public DbConnection Connection => dbConnection;

        public bool IsUsingSharedConnection => isUsingSharedConnection;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(!isUsingSharedConnection)
                    {
                        dbConnection.Dispose();
                    }

                    disposedValue = true;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AutomaticConnectionScope()
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

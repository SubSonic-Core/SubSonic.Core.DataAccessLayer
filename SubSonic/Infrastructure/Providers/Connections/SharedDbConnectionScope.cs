using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Infrastructure
{
    class SharedDbConnectionScope
        : IDisposable
    {
        [ThreadStatic]
        private static Stack<SharedDbConnectionScope> __instances;

        private readonly DbDatabase dbDatabase;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public SharedDbConnectionScope(DbDatabase dbDatabase)
        {
            this.dbDatabase = dbDatabase ?? throw new ArgumentNullException(nameof(dbDatabase));
            this.dbDatabase.InitializeSharedConnection();

            if(__instances == null)
            {
                __instances = new Stack<SharedDbConnectionScope>();
            }
            __instances.Push(this);
        }

        public DbConnection CurrentConnection => this.dbDatabase.CurrentSharedConnection;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    __instances.Pop();

                    if(__instances.Count == 0)
                    {
                        this.dbDatabase.ResetSharedConnection();
                    }

                    disposedValue = true;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SharedDbConnectionScope()
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
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

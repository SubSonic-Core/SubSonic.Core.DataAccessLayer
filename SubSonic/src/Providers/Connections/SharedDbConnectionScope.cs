using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic
{
    public class SharedDbConnectionScope
        : IConnectionScope
        , IDisposable
    {
        //[ThreadStatic]
        private static Dictionary<int, Stack<SharedDbConnectionScope>> __db_instances;

        private readonly DbDatabase dbDatabase;

        public SharedDbConnectionScope(DbDatabase dbDatabase)
        {
            this.dbDatabase = dbDatabase ?? throw new ArgumentNullException(nameof(dbDatabase));
            this.dbDatabase.InitializeSharedConnection();
            this.threadId = Thread.CurrentThread.ManagedThreadId;

            if (__db_instances == null)
            {
                __db_instances = new Dictionary<int, Stack<SharedDbConnectionScope>>();
            }

            if (!__db_instances.ContainsKey(threadId))
            {
                __db_instances[threadId] = new Stack<SharedDbConnectionScope>();
            }

            __db_instances[threadId].Push(this);
        }

        private readonly int threadId;

        public DbConnection Connection => dbDatabase.CurrentSharedConnection;

        public DbDatabase Database => dbDatabase;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SharedDbConnectionScope scope = __db_instances[threadId].Pop();

                    Debug.Assert(scope.dbDatabase.CurrentSharedConnection.State == System.Data.ConnectionState.Closed, "open connection is being disposed.");

                    if (__db_instances[threadId]?.Count == 0)
                    {
                        __db_instances.Remove(threadId);
                    }

                    if (__db_instances.Count == 0)
                    {
                        scope.dbDatabase.ResetSharedConnection();
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
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

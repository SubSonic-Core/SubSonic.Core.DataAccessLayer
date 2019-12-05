using SubSonic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SubSonic.Tests", AllInternalsVisible = true)]

namespace SubSonic
{
    public class DbContext
        : IDisposable, IInfrastructure<IServiceProvider>
    {
        protected DbContext()
        {

        }

        public DbSet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return new DbSet<TEntity>(this);
        }

        protected virtual void OnDbConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected virtual void OnDbModeling(DbModelBuilder modelBuilder)
        {

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DbContext()
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

        public IServiceProvider Instance => throw new NotImplementedException();
    }
}

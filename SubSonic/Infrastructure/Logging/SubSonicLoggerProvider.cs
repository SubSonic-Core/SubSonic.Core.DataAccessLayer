using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Logging
{
    public class SubSonicLoggerProvider
        : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers;
        private readonly IServiceProvider provider;

        public SubSonicLoggerProvider(IServiceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            _loggers = new ConcurrentDictionary<string, ILogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, category =>
            {
                Type
                    generic = Type.GetType(category) ?? Type.GetType($"{category}`1") ?? Type.GetType($"{category}`2"),
                    subSonicLogger = typeof(SubSonicLogger<>).MakeGenericType(generic),
                    logger = typeof(ILogger<>).MakeGenericType(generic);

                return (ILogger)Activator.CreateInstance(subSonicLogger, provider.GetService(logger));
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _loggers.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SubSonicLoggerProvider()
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

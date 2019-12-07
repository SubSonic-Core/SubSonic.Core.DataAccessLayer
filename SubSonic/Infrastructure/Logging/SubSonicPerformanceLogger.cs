using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Infrastructure.Logging
{
    public class SubSonicPerformanceLogger<CategoryName>
        : IPerformanceLogger<CategoryName>
        , IDisposableAsync
    {
        private readonly DateTime start;
        private readonly ILogger<CategoryName> logger;
        private readonly string name;

        public SubSonicPerformanceLogger(ILogger<CategoryName> logger, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("message", nameof(name));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.name = name;

            if (IsPerformanceLoggingEnabled)
            {
                start = DateTime.Now;

                logger.LogDebug("Start Execution of {name} at {time}", $"{typeof(CategoryName).Name}::{name}", start);
            }
        }

        public bool IsPerformanceLoggingEnabled => logger.IsEnabled(LogLevel.Debug);

        public async Task DisposeAsync()
        {
            if (IsPerformanceLoggingEnabled)
            {
                DateTime end = DateTime.Now;

                logger.LogDebug("End Execution of {name} at {time} elapsed time: {milliseconds} ms", $"{typeof(CategoryName).Name}::{name}", end, (end - start).TotalMilliseconds);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Task.Factory.StartNew(async () => await DisposeAsync());
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SubSonicPerformanceLogger()
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

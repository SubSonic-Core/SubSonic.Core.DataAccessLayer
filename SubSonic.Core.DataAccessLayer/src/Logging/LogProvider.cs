using Microsoft.Extensions.Logging;
using SubSonic.Core.Logging;
using SubSonic.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Logging
{
    public class LogProvider<TClassName, TLogger>
        : ILoggerProvider
        where TLogger : Logger, new()
    {
        private readonly ConcurrentDictionary<string, ILogger> m_loggers;
        private bool disposedValue;

        protected LogProvider()
        {
            this.m_loggers = new ConcurrentDictionary<string, ILogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return m_loggers.GetOrAdd($"{typeof(TClassName).Name}::{categoryName}", new TLogger());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_loggers.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~LogProvider()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

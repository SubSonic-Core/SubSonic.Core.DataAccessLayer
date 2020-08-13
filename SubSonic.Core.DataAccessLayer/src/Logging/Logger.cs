using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Logging
{
    public class Logger
        : ILogger
    {
        private readonly LogLevel logLevel;

        protected Logger(LogLevel logLevel)
        {
            this.logLevel = logLevel;
        }

        public virtual IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= this.logLevel;
        }

        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new NotImplementedException();
        }
    }
}

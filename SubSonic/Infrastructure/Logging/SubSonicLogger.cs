using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Logging
{
    class SubSonicLogger<TCategoryName>
        : ISubSonicLogger<TCategoryName>
    {
        private readonly ILogger logger;

        public SubSonicLogger(ILogger<TCategoryName> logger)
        {
            this.logger = logger;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logger.IsNotNull() && logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        public IPerformanceLogger<TCategoryName> Start(string name)
        {
            return new SubSonicPerformanceLogger<TCategoryName>(logger, name);
        }
    }
}

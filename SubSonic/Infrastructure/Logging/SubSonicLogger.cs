using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Logging
{
    public class SubSonicLogger<CategoryName>
        : ISubSonicLogger<CategoryName>
    {
        private readonly ILogger<CategoryName> logger;

        public SubSonicLogger(ILogger<CategoryName> logger)
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
                logger.Log<TState>(logLevel, eventId, state, exception, formatter);
            }
        }

        public IPerformanceLogger<CategoryName> Start(string name)
        {
            return new SubSonicPerformanceLogger<CategoryName>(logger, name);
        }
    }
}

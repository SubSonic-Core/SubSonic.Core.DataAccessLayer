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
        private readonly ITraceLogger<TCategoryName> trace;
        private Type categoryType;

        public SubSonicLogger(ILogger<TCategoryName> logger)
        {
            this.logger = logger;
            this.trace = new SubSonicTraceLogger<TCategoryName>(logger);
            this.categoryType = typeof(TCategoryName);
        }

        public ILogger Write => this.logger;

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

        public IPerformanceLogger Start(Type categoryType, string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.categoryType = categoryType ?? throw new ArgumentNullException(nameof(categoryType));

            return ((ISubSonicLogger)this).Start(name);
        }

        IPerformanceLogger ISubSonicLogger.Start(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new SubSonicPerformanceLogger(categoryType, logger, name);
        }

        public void Trace(string method, string message)
        {
            trace.WriteTrace(method, message);
        }
    }
}

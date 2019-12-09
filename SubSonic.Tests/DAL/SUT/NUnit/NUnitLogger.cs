using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Tests.DAL.SUT.NUnit
{
    public class NUnitLogger<TClassName>
        : ILogger
    {
        private readonly LogLevel logLevel;

        public NUnitLogger(LogLevel logLevel)
        {
            this.logLevel = logLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= this.logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            TestContext.WriteLine(formatter(state, exception));
        }
    }
}

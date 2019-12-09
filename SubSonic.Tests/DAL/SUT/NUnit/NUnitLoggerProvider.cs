using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Tests.DAL.SUT.NUnit
{
    public sealed class NUnitLoggerProvider<TClassName>
        : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers;
        private readonly LogLevel logLevel;

        public NUnitLoggerProvider(LogLevel logLevel)
        {
            this.logLevel = logLevel;
            _loggers = new ConcurrentDictionary<string, ILogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd($"{typeof(TClassName).Name}::{categoryName}", new NUnitLogger<TClassName>(logLevel));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Tests.DAL.SUT.NUnit
{
    public class NUnitLoggerProvider
        : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers;

        public NUnitLoggerProvider()
        {
            _loggers = new ConcurrentDictionary<string, ILogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, new NUnitLogger());
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}

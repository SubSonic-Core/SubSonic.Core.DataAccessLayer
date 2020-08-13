using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace SubSonic.Logging
{
    public class DebugLogger
        : Logger
    {
        public DebugLogger()
            : base(LogLevel.Debug) { }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (IsEnabled(logLevel))
            {
                Debug.WriteLine(formatter(state, exception));
            }
        }
    }
}

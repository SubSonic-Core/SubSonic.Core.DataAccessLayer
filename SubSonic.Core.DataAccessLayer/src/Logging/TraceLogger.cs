using Microsoft.Extensions.Logging;
using SubSonic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SubSonic.Core.Logging
{
    public class TraceLogger
        : Logger
    {
        public TraceLogger()
            : base(LogLevel.Trace) { }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (IsEnabled(logLevel))
            {
                Trace.WriteLine(formatter(state, exception));
            }
        }
    }
}

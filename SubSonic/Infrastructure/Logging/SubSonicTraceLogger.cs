using Microsoft.Extensions.Logging;
using System;

namespace SubSonic.Infrastructure.Logging
{
    using Linq;

    class SubSonicTraceLogger<TCategoryName>
        : ITraceLogger<TCategoryName>
    {
        private readonly ILogger logger;

        public SubSonicTraceLogger(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsTraceLoggingEnabled => logger.IsNotNull() && logger.IsEnabled(LogLevel.Trace);

        public void WriteTrace(string method,
                               string message)
        {
            if (IsTraceLoggingEnabled)
            {
                if (string.IsNullOrEmpty(method))
                {
                    throw new ArgumentException("", nameof(method));
                }

                logger.LogTrace(SubSonicLogging.Trace, method, message);
            }
        }
    }
}

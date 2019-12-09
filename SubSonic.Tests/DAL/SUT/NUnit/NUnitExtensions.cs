using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Tests.DAL.SUT.NUnit
{
    static class NUnitExtensions
    {
        public static ILoggingBuilder AddNUnitLogger<TClassName>(this ILoggingBuilder builder, LogLevel logLevel)
        {
            builder
                .ClearProviders()
                .AddProvider(new NUnitLoggerProvider<TClassName>(logLevel))
                .SetMinimumLevel(logLevel);

            return builder;
        }
    }
}

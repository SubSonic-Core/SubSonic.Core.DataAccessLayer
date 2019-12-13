using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SubSonic.Infrastructure.Logging
{
    public interface ISubSonicLogger<out TCategoryName>
        : ISubSonicLogger
    {
        IPerformanceLogger<TCategoryName> Start(string name);
    }

    public interface ISubSonicLogger
        : ILogger
    {
        ILogger Write { get; }

        IPerformanceLogger Start(Type category, string name);

        IPerformanceLogger Start(string name);

        void Trace(string method, string message);
    }

}

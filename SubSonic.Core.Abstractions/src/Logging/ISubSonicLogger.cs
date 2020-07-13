using Microsoft.Extensions.Logging;
using System;

namespace SubSonic.Logging
{
    public interface ISubSonicLogger<out TCategoryName>
        : ISubSonicLogger
    {
        new IPerformanceLogger<TCategoryName> Start(string name);
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

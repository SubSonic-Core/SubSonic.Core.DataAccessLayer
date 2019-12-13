using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Infrastructure.Logging
{
    public interface IPerformanceLogger<out TCategoryName>
        : IPerformanceLogger
    {

    }

    public interface IPerformanceLogger
        : IDisposable
    {
        bool IsPerformanceLoggingEnabled { get; }
        double TotalMilliseconds { get; }
        double TotalSeconds { get; }
        double TotalMinutes { get; }
        string NameOfScope { get; }

        void StartClock(string name);
        void EndClock();
    }
}

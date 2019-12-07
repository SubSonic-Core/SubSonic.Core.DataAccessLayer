using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Infrastructure.Logging
{
    public interface IPerformanceLogger<out CategoryName>
        : IDisposableAsync
    {
        bool IsPerformanceLoggingEnabled { get; }
        double TotalMilliseconds { get; }
        double TotalSeconds { get; }
        double TotalMinutes { get; }
        string NameOfScope { get; }

        void Start(string name);
        void End();
        /// <summary>
        /// Sets the End Date for the performance logger
        /// </summary>
        /// <returns></returns>
        Task EndAsync();
    }
}

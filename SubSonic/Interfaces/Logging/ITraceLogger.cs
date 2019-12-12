using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Logging
{
    public interface ITraceLogger<out TCategoryName>
    {
        bool IsTraceLoggingEnabled { get; }

        void WriteTrace(string method, string message);
    }
}

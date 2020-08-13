using SubSonic.Core.Logging;
using SubSonic.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Logging
{
    public class DebugLogProvider<TClassName>
        : LogProvider<TClassName, DebugLogger>
    {
        public DebugLogProvider()
            : base() { }
    }

    public class TraceLogProvider<TClassName>
        : LogProvider<TClassName, TraceLogger>
    {
        public TraceLogProvider()
            : base() { }
    }
}

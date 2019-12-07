using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SubSonic.Infrastructure.Logging
{
    public interface ISubSonicLogger<out CategoryName>
        : ILogger<CategoryName>
    {
        IPerformanceLogger<CategoryName> Start(string name);
    }
}

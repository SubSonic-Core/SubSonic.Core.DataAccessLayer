using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Infrastructure
{
    public interface IDisposableAsync
        : IDisposable
    {
        Task DisposeAsync();
    }
}

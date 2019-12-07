using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Infrastructure
{
    public interface ISubSonicQueryProvider
        : IQueryProvider
    {
        DbContext DbContext { get; }
    }
}

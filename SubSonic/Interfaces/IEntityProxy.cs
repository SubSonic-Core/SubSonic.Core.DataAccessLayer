using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public interface IEntityProxy
    {
        bool IsDirty { get; }
    }
}

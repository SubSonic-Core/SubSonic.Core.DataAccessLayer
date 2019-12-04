using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public interface IInfrastructure<out TType>
    {
        TType Instance { get; }
    }
}

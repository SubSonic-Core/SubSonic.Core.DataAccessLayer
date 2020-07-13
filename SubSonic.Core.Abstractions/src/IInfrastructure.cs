using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public interface IInfrastructure<out TType>
    {
        TType Instance { get; }
    }
}

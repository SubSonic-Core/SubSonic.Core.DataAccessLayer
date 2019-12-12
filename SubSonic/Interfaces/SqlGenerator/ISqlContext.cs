using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    public interface ISqlContext
    {
        ISqlFragment Fragments { get; }

        ISqlMethods Methods { get; }
    }
}

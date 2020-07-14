using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.SqlGenerator
{
    using Factory;

    public interface ISqlContext
    {
        ISqlFragment Fragments { get; }

        ISqlMethods Methods { get; }

        SubSonicDbProvider Provider { get; }
    }
}

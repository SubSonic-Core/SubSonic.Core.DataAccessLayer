using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Interfaces
{
    internal interface IDbParameterProvider
    {
        IReadOnlyCollection<DbParameter> Parameters { get; }
    }
}

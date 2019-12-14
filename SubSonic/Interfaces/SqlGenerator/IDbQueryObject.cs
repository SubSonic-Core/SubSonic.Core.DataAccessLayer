using SubSonic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public interface IDbQueryObject
    {
        string Sql { get; }
        IReadOnlyCollection<SubSonicParameter> Parameters { get; }
    }
}

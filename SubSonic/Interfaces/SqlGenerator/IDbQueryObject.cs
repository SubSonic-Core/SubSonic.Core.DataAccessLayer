using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace SubSonic
{
    public interface IDbQueryObject
    {
        string Sql { get; }
        IReadOnlyCollection<DbParameter> Parameters { get; }
        CommandBehavior Behavior { get; }
    }
}

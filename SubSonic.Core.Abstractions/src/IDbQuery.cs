using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace SubSonic
{
    public interface IDbQuery
    {
        string Sql { get; }
        IReadOnlyCollection<DbParameter> Parameters { get; }
        CommandBehavior Behavior { get; }

        void CleanUpParameters();
    }
}

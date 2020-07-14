using System.Collections.Generic;
using System.Data.Common;

namespace SubSonic.Linq.Expressions
{
    public interface IDbParameterProvider
    {
        IReadOnlyCollection<DbParameter> Parameters { get; }
    }
}

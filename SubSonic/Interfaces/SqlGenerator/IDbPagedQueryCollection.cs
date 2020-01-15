using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public interface IDbPagedQueryCollection<TEntity>
        : IDbPagedQuery
        , IEnumerable<IEnumerable<TEntity>>
    {

    }
}

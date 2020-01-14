using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Interfaces
{
    public interface IDbPagedQueryCollection<TEntity>
        : IDbPagedQuery
        , IEnumerable<IEnumerable<TEntity>>
    {

    }
}

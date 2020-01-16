using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public interface IDbPagesCollection<out TEntity>
        : IEnumerable<IDbPageCollection<TEntity>>
    {

    }
}

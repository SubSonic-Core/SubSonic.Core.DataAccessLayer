using System.Collections.Generic;

namespace SubSonic
{
    public interface IDbPagesCollection<out TEntity>
        : IEnumerable<IDbPageCollection<TEntity>>
    {

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Infrastructure
{
    public interface ISubSonicDbSetCollectionProvider<out TEntity>
        : IQueryProvider
    {
        DbContext DbContext { get; }
    }
}

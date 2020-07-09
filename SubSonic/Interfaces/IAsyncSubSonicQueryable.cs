using System.Collections.Generic;
using System.Linq;

namespace SubSonic.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IAsyncSubSonicQueryable<out TEntity>
        : IAsyncEnumerable<TEntity>
        , IQueryable<TEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        IAsyncSubSonicQueryProvider ProviderAsync { get; }
    }
}

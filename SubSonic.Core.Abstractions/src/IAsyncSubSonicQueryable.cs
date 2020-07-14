using System.Collections.Generic;
using System.Linq;

namespace SubSonic
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

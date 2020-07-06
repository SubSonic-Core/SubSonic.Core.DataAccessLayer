using System.Collections.Generic;
using System.Linq;

namespace SubSonic.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IAsyncSubSonicQueryable<out TEntity>
        : IQueryable, IEnumerable<TEntity>, IAsyncEnumerable<TEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        IAsyncSubSonicQueryProvider AsyncProvider { get; }
    }
}

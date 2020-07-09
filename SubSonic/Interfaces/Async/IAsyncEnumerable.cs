using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SubSonic.Collections
{
    /// <summary>
    /// Provide definition for the an asynchronous enumerator
    /// </summary>
    public interface IAsyncEnumerable
    {
        /// <summary>
        /// Gets an asynchronous enumerator, that supports iteration on a non-generic collection
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerator GetAsyncEnumerator(CancellationToken cancellationToken = default);
    }

#if !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NET461
    /// <summary>
    /// Gets an asynchronous enumerator, that supports iteration on a generic collection
    /// </summary>
    /// <typeparam name="TEntity">The type of items in the collection</typeparam>
    public interface IAsyncEnumerable<out TEntity> : IAsyncEnumerable
    {
        IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default);
    }
#endif
}

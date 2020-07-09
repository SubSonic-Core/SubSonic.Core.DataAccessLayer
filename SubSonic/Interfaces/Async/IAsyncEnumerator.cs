using System.Threading.Tasks;

namespace SubSonic.Collections
{
    public interface IAsyncEnumerator
    {
        /// <summary>
        /// Current element in the collection
        /// </summary>
        object Current { get; }

        /// <summary>
        /// advances the enumerator to the next element asynchronously
        /// </summary>
        /// <returns></returns>
        ValueTask<bool> MoveNextAsync();
    }

#if !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NET461
    /// <summary>
    /// Supports a simple asynchronous iteration over a collection of typed items
    /// </summary>
    /// <typeparam name="TEntity">The type of items in the collection</typeparam>
    public interface IAsyncEnumerator<out TEntity> : IAsyncEnumerator
    {
        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        new TEntity Current { get; }
    }
#endif
}

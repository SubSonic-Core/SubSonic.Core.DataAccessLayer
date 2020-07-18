using Dasync.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SubSonic.Collections
{
    using System.Threading.Tasks;

    public partial class SubSonicCollection<TEntity>
        : IAsyncSubSonicQueryable<TEntity>
    {
        public IAsyncSubSonicQueryProvider ProviderAsync
        {
            get
            {
                if (Provider is IAsyncSubSonicQueryProvider provider)
                {
                    return provider;
                }

                return null;
            }
        }

#if NETSTANDARD2_0
        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new AsyncEnumerable<TEntity>(async (yield) =>
            {
                foreach (TEntity entity in this)
                {
                    await yield.ReturnAsync(entity).ConfigureAwait(false);
                }
            }).GetAsyncEnumerator(cancellationToken);
        }
#elif NETSTANDARD2_1
        public async IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            await foreach(TEntity entity in this.WithCancellation(cancellationToken))
            {
                yield return entity;
            }
        }
#endif
    }
}

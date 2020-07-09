using Dasync.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SubSonic.Collections
{
    using Interfaces;

    public sealed partial class SubSonicCollection<TEntity>
        : IAsyncSubSonicQueryable<TEntity>
    {
        IAsyncSubSonicQueryProvider IAsyncSubSonicQueryable<TEntity>.ProviderAsync
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

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new AsyncEnumerable<TEntity>(async (yield) =>
            {
                foreach (TEntity entity in this)
                {
                    await yield.ReturnAsync(entity).ConfigureAwait(false);
                }
            }).GetAsyncEnumerator(cancellationToken);
        }
    }
}

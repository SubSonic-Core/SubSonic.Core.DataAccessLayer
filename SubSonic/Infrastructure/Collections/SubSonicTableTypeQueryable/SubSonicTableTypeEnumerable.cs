using Dasync.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SubSonic.Collections
{
    using Interfaces;

    public sealed partial class SubSonicTableTypeCollection<TElement>
        : IAsyncSubSonicQueryable<TElement>
    {
        IAsyncSubSonicQueryProvider IAsyncSubSonicQueryable<TElement>.ProviderAsync
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

        IAsyncEnumerator<TElement> IAsyncEnumerable<TElement>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new AsyncEnumerable<TElement>(async (yield) =>
            {
                foreach (TElement entity in this)
                {
                    await yield.ReturnAsync(entity).ConfigureAwait(false);
                }
            }).GetAsyncEnumerator(cancellationToken);
        }
    }
}

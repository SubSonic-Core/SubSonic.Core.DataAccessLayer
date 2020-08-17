using Dasync.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace SubSonic.Collections
{
    public sealed partial class SubSonicSetCollection<TEntity>
        : IEnumerator<TEntity>
        where TEntity: class
    {
        private TEntity current;

        private int position;

        TEntity IEnumerator<TEntity>.Current => current;

        object IEnumerator.Current => current;

        public IEnumerator GetEnumerator()
        {
            if (!isLoaded)
            {
                Load();
            }

            return this;
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            if (!isLoaded)
            {
                Load();
            }

            return this;
        }

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new AsyncEnumerable<TEntity>(async yield =>
            {
                foreach (TEntity entity in this)
                {
                    await yield
                    .ReturnAsync(entity)
                        .ConfigureAwait(false);
                }
            }).GetAsyncEnumerator(cancellationToken);
        }

        bool IEnumerator.MoveNext()
        {
            if (position < dataset.Count)
            {
                current = dataset.ElementAt(position).Data;

                position++;
            }
            else
            {
                current = null;
            }
            
            return !(current is null);
        }

        void IEnumerator.Reset()
        {
            position = 0;
        }

        public void Dispose()
        {
            if (this is IEnumerator enumerator)
            {
                enumerator.Reset();
                current = null;
            }
        }
    }
}

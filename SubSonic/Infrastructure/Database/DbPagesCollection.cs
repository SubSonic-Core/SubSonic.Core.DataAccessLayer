using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbPagesCollection<TEntity>
        : IDbPagesCollection<TEntity>
    {
        private readonly DbPageCollection<TEntity> page;
        public DbPagesCollection(DbPageCollection<TEntity> page)
        {
            this.page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public IEnumerator<IDbPageCollection<TEntity>> GetEnumerator()
        {
            return new DbPagesEnumerator<TEntity>(page);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

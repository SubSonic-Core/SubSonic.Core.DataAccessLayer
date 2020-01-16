using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbPagesEnumerator<TEntity>
        : IEnumerator<IDbPageCollection<TEntity>>
    {
        private readonly DbPageCollection<TEntity> page;

        public DbPagesEnumerator(DbPageCollection<TEntity> page)
        {
            this.page = page;
        }

        public IDbPageCollection<TEntity> Current
        {
            get
            {
                page.PreFetch();

                return page;
            }
        }
        

        object IEnumerator.Current => this;

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            if (page.PageNumber < page.PageCount ||
                page.PageNumber == 0)
            {
                ++page.PageNumber;

                return true;
            }
            return false;
        }

        public void Reset()
        {
            page.PageNumber = 1;
        }
    }
}

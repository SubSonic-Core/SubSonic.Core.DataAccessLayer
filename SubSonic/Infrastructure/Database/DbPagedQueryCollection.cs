using SubSonic.Linq.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SubSonic.Infrastructure
{
    internal class DbPagedQueryCollection<TEntity>
        : DbPagedQuery
        , IDbPagedQueryCollection<TEntity>
    {
        public DbPagedQueryCollection(DbSelectExpression select, int size)
            : base(select, size) { }

        public DbPagedQueryCollection(DbSelectPagedExpression select)
            : base(select) { }

        public IEnumerator<IEnumerable<TEntity>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

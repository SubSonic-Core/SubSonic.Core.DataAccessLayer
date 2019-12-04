using SubSonic.Infrastructure.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbSet<TEntity>
        : IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable, IListSource
        where TEntity : class
    {
        private readonly DbContext dbContext;
        private readonly List<TEntity> source;

        public DbSet(DbContext dbContext)
        {
            this.dbContext = dbContext;
            this.source = new List<TEntity>();
        }

        public Type ElementType => typeof(TEntity);

        public Expression Expression => Expression.Default(ElementType);

        public IQueryProvider Provider => new SubSonicQueryProvider(dbContext);

        public bool ContainsListCollection => true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerator GetEnumerator()
        {
            return GetList().GetEnumerator();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IList GetList()
        {
            return source;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return ((IEnumerable<TEntity>)source).GetEnumerator();
        }
    }
}

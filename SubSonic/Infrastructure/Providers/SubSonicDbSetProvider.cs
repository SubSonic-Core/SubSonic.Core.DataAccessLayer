using SubSonic.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure.Providers
{
    public class SubSonicDbSetCollectionProvider<TEntity>
        : ISubSonicDbSetCollectionProvider<TEntity>
    {
        private readonly ISubSonicLogger<DbSetCollection<TEntity>> logger;

        public SubSonicDbSetCollectionProvider(DbContext dbContext, ISubSonicLogger<DbSetCollection<TEntity>> logger)
        {
            this.DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public DbContext DbContext { get; }

        public IQueryable CreateQuery(Expression expression)
        {
            using (IPerformanceLogger<DbSetCollection<TEntity>> performance = logger.Start(nameof(CreateQuery)))
            {
                throw new NotImplementedException();
            }
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            using (IPerformanceLogger<DbSetCollection<TEntity>> performance = logger.Start($"{nameof(CreateQuery)}<{typeof(TElement).GetTypeName()}>"))
            {
                return new DbSetCollection<TElement>((ISubSonicDbSetCollectionProvider<TElement>)this, (Expression)expression);
            }
        }

        public object Execute(Expression expression)
        {
            using (IPerformanceLogger<DbSetCollection<TEntity>> performance = logger.Start(nameof(Execute)))
            {
                throw new NotImplementedException();
            }
        }

        public virtual TResult Execute<TResult>(Expression expression)
        {
            using (IPerformanceLogger<DbSetCollection<TEntity>> performance = logger.Start($"{nameof(Execute)}<{typeof(TResult).GetTypeName()}>"))
            {
                return DbContext.Database.ExecuteQuery<TResult>(expression);
            }
        }
    }
}

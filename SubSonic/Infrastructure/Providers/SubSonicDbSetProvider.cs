using SubSonic.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure.Providers
{
    public class SubSonicDbSetProvider<TEntity>
        : ISubSonicQueryProvider
    {
        private readonly ISubSonicLogger<DbSet<TEntity>> logger;

        public SubSonicDbSetProvider(DbContext dbContext, ISubSonicLogger<DbSet<TEntity>> logger)
        {
            this.DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public DbContext DbContext { get; }

        public IQueryable CreateQuery(Expression expression)
        {
            using (IPerformanceLogger<DbSet<TEntity>> performance = logger.Start(nameof(CreateQuery)))
            {
                throw new NotImplementedException();
            }
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            using (IPerformanceLogger<DbSet<TEntity>> performance = logger.Start($"{nameof(CreateQuery)}<{typeof(TElement).GetQualifiedTypeName()}>"))
            {
                return new DbSet<TElement>(this, expression);
            }
        }

        public object Execute(Expression expression)
        {
            using (IPerformanceLogger<DbSet<TEntity>> performance = logger.Start(nameof(Execute)))
            {
                throw new NotImplementedException();
            }
        }

        public virtual TResult Execute<TResult>(Expression expression)
        {
            using (IPerformanceLogger<DbSet<TEntity>> performance = logger.Start($"{nameof(Execute)}<{typeof(TResult).GetQualifiedTypeName()}>"))
            {
                throw new NotImplementedException();
            }
        }
    }
}

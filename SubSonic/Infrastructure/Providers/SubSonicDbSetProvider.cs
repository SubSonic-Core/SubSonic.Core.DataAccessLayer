using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure.Providers
{
    public class SubSonicDbSetProvider
        : ISubSonicQueryProvider
    {
        public SubSonicDbSetProvider(DbContext dbContext)
        {
            this.DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public DbContext DbContext { get; }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new DbSet<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public virtual TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}

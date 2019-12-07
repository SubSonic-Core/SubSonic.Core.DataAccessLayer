using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure.Providers
{
    public class SubSonicDbSetProvider
        : IQueryProvider
    {
        private readonly DbContext dbContext;

        public SubSonicDbSetProvider(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new DbSet<TElement>(dbContext, this, expression);
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

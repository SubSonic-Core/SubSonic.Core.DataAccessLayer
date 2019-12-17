using System;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure.Builders
{
    using Logging;
    using System.Data.Common;

    public partial class DbSqlQueryBuilder
    {
        public IQueryable CreateQuery(Expression expression)
        {
            using (IPerformanceLogger performance = logger.Start(GetType(), nameof(CreateQuery)))
            {
                return new SubSonicCollection(DbEntity.EntityModelType, this, BuildQuery(expression));
            }
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
        {
            return new SubSonicCollection<TEntity>(this, BuildQuery(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        public object Execute(Expression expression)
        {
            using (AutomaticConnectionScope Scope = DbContext.ServiceProvider.GetService<AutomaticConnectionScope>())
            using (var perf = logger.Start(GetType(), nameof(Execute)))
            {
                DbCommand cmd = Scope.Connection.CreateCommand();

                throw new NotImplementedException();
            }
        }
    }
}

using SubSonic.Logging;

namespace SubSonic.Builders
{
    using Linq.Expressions;

    public class DbSqlQueryBuilder<TEntity>
        : DbSqlQueryBuilder
        , ISubSonicQueryProvider<TEntity>
    {
        public DbSqlQueryBuilder(ISubSonicLogger<TEntity> logger)
            : base(typeof(TEntity), logger)
        {
        }
    }
}

namespace SubSonic.Infrastructure.Builders
{
    using Linq.Expressions;
    using Logging;

    public class DbSqlQueryBuilder<TEntity>
        : DbSqlQueryBuilder
        , ISubSonicQueryProvider<TEntity>
    {
        public DbSqlQueryBuilder(ISubSonicLogger<TEntity> logger)
            : base(typeof(TEntity), logger)
        {
        }

        public DbTableExpression GetDbTable() => DbTable;
    }
}

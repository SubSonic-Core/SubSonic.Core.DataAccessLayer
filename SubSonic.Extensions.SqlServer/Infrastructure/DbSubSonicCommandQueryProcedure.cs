using System.Collections.Generic;
using System.Data;

namespace SubSonic.Infrastructure
{
    using Extensions.SqlServer;
    using Linq;

    public class DbSubSonicCommandQueryProcedure<TEntity>
        : DbSubSonicStoredProcedure
        where TEntity: class
    {
        public DbSubSonicCommandQueryProcedure(IEnumerable<IEntityProxy> entities)
        {
            Entities = entities.Select(x =>
            {
                if (x is IEntityProxy<TEntity> property)
                {
                    return property.Data;
                }
                return null;
            })
                .Where(x => x.IsNotNull())
                .ToArray();
        }

        [DbSqlParameter(nameof(Entities), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Structured)]
        public IEnumerable<TEntity> Entities { get; }

        [DbSqlParameter(nameof(Result), Direction = ParameterDirection.ReturnValue, SqlDbType = SqlDbType.Int)]
        public override int Result { get; set; }
    }
}

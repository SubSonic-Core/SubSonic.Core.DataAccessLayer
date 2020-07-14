using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SubSonic
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

        [DbSqlParameter(nameof(Error), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.VarChar)]
        public override string Error { get => base.Error; set => base.Error = value; }

        [DbSqlParameter(nameof(Result), Direction = ParameterDirection.ReturnValue, SqlDbType = SqlDbType.Int)]
        public override int Result { get; set; }
    }
}

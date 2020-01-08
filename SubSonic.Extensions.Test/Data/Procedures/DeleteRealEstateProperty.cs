using SubSonic.Extensions.SqlServer;
using SubSonic.Infrastructure;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SubSonic.Extensions.Test
{
    [DbStoredProcedure(nameof(DeleteRealEstateProperty))]
    public class DeleteRealEstateProperty
    {
        public DeleteRealEstateProperty(IEnumerable<IEntityProxy> properties)
        {
            Properties = properties.Select(x =>
            {
                if (x is IEntityProxy<Models.RealEstateProperty> property)
                {
                    return property.Data;
                }
                return null;
            })
                .Where(x => x.IsNotNull())
                .ToArray();
        }

        [DbSqlParameter(nameof(Properties), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Structured)]
        public IEnumerable<Models.RealEstateProperty> Properties { get; }
    }
}
